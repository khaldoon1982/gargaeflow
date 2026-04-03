# API & Database Plan for GarageFlow

## Current Situation

GarageFlow is a local Windows desktop application (WPF, .NET 8, Clean Architecture).
The app uses a local SQLite database via EF Core 8.0.11.

### What works today

| Component | Status |
|---|---|
| WPF desktop app | Fully functional |
| Local SQLite database | Working (garageflow.db) |
| 9 entity tables | Customer, Vehicle, MaintenanceRecord, Inspection, Reminder, User, Part, AuditLog, AppSetting |
| Soft delete (ISoftDeletable) | Active on all main entities |
| Audit timestamps (BaseEntity) | CreatedAt + UpdatedAt on all entities |
| Generic repository pattern | IRepository\<T\> with open generic DI |
| EF Core migrations | Code-first, auto-applied at startup |
| Database seeding | Sample data for testing |
| Backup/restore | File copy of .db file |
| Login + role system | Admin, Technician, Receptionist, Viewer |
| Settings system | Key-value AppSettings table |
| .env.local | Created (desktop config only, sync disabled) |
| .gitignore | Configured (protects .env, .db, logs) |

---

## Architecture Rule

The Windows desktop app must **never** connect directly to PostgreSQL.

### Desktop app knows only

- Local SQLite database
- API URL
- API key
- Device identifier
- Sync settings

### API server knows only

- PostgreSQL connection
- Server secrets
- Cloud database credentials

---

## Target Architecture

```
┌─────────────────────────────────────────────────────┐
│                   DESKTOP (WPF)                     │
│                                                     │
│  User ──> Services ──> Repository ──> SQLite        │
│                                         │           │
│                              SaveChanges intercept  │
│                                         │           │
│                                    SyncQueue        │
│                                         │           │
│                              BackgroundSyncService  │
│                                         │           │
│                                    HTTP Push/Pull   │
└─────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│                 API SERVER (ASP.NET Core)            │
│                                                     │
│  POST /api/sync/push   ──> SyncProcessor            │
│  POST /api/sync/pull   ──> SyncProcessor            │
│  GET  /api/sync/status ──> Sync state + metadata    │
│  GET  /health          ──> Health check             │
│  POST /api/auth/device ──> Device registration      │
│                                   │                 │
│                              ApiDbContext            │
│                                   │                 │
│                              PostgreSQL             │
└─────────────────────────────────────────────────────┘
```

---

## Data Flow

1. User works in the desktop app
2. Data saves to local SQLite (always works offline)
3. SaveChangesAsync intercept detects ISyncable changes
4. SyncQueueEntry rows created automatically
5. BackgroundSyncService picks up queue entries on timer
6. Push: sends changes to API (ordered by FK dependencies)
7. API writes to PostgreSQL, confirms with version numbers
8. Pull: API returns changes from other devices
9. Local entities updated with SyncStatus = Synced

---

## Environment Files

### Desktop app: `.env.local`

**Development**: solution root
**Production install**: `%LocalAppData%\GarageFlow\config.env`

```env
GARAGEFLOW_SYNC_ENABLED=false
GARAGEFLOW_SYNC_INTERVAL_SECONDS=60
GARAGEFLOW_API_URL=https://api.jouwdomein.nl
GARAGEFLOW_API_KEY=change-me-to-a-real-key
GARAGEFLOW_DEVICE_ID=auto
GARAGEFLOW_SQLITE_PATH=garageflow.db
GARAGEFLOW_LOG_LEVEL=Information
```

No PostgreSQL credentials. No server secrets.

**Note**: `GARAGEFLOW_SQLITE_PATH=garageflow.db` is relative and fine for development. For a production desktop install, the database should be stored in the user's AppData folder: `%LocalAppData%\GarageFlow\garageflow.db`

### API server: `.env.production` (on server / Coolify)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__Default=Host=garageflow-postgres;Port=5432;Database=garageflow_prod;Username=garageflow_user;Password=YOUR_STRONG_PASSWORD
GARAGEFLOW_API_KEY=YOUR_REAL_API_KEY
GARAGEFLOW_LOG_LEVEL=Information
```

PostgreSQL credentials only here. Never on the desktop.

---

## Syncable Entities

Only 5 of 9 entities sync to the cloud:

| Entity | FK Dependencies | Sync Level |
|---|---|---|
| Customer | None | 0 (sync first) |
| Vehicle | Customer | 1 |
| MaintenanceRecord | Vehicle | 2 |
| Inspection | Vehicle | 2 |
| Reminder | Customer (required), Vehicle (optional) | 2 |

### Non-syncable entities

| Entity | Reason | Future |
|---|---|---|
| User | Local per device in MVP | Central user identity via API in later phase |
| Part | Linked to MaintenanceRecord (cascade) | Will sync in phase 2 if cloud reporting needs cost/parts detail |
| AuditLog | Local audit trail | May sync for compliance later |
| AppSetting | Device-specific config | Not needed in cloud |

---

## Sync Fields (ISyncable interface)

Added to the 5 syncable entities:

| Field | Type | Purpose |
|---|---|---|
| CloudId | Guid (not null) | **Client-generated** on entity creation, used as cross-device identity |
| SyncStatus | enum | Synced, PendingUpload, PendingDownload, Conflict |
| LastSyncedAtUtc | DateTime? | When last synced to cloud |
| LastLocalChangeAtUtc | DateTime? | When last changed locally |
| VersionNumber | int | Optimistic concurrency (starts at 1) |
| DeviceId | string? | Which device made the change |

**Important**: CloudId is generated locally (Guid.NewGuid()) when the entity is created. The same ID is sent to the server. This works cleanly offline, avoids post-upload mapping, and enables idempotent retries.

---

## SyncQueue Table

| Field | Type | Purpose |
|---|---|---|
| Id | int | PK |
| EntityName | string | "Customer", "Vehicle", etc. |
| EntityId | int | Local PK of the changed entity |
| OperationType | enum | Create, Update, Delete |
| CreatedAtUtc | DateTime | When queued |
| Status | enum | Pending, Processing, Completed, Failed |
| RetryCount | int | Number of failed attempts |
| ErrorMessage | string? | Last error details |
| ProcessedAtUtc | DateTime? | When completed/failed |

### Enqueue Rules

The SaveChangesAsync interception must:
- Only enqueue for Added, Modified, and soft-delete transitions on ISyncable entities
- NOT enqueue if only sync metadata fields changed (SyncStatus, LastSyncedAtUtc, VersionNumber)
- NOT enqueue during pull/apply from server (IsSyncSuppressed flag)

---

## Conflict Resolution

**Optimistic concurrency with server-authoritative resolution:**

1. Client pushes entity with its VersionNumber
2. Server checks: does client VersionNumber match the server's current version?
3. If match → accept, increment version, return new version number
4. If mismatch → reject, return conflict with server's current data
5. Client marks entity SyncStatus = Conflict
6. Future: UI for manual conflict resolution

This is deterministic. Conflicts are rare in a 1-3 device garage setup.

---

## API Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /health` | Infrastructure health check (uptime, DB connectivity) |
| `GET /api/sync/status` | Lightweight sync metadata: last sync time, server version. Not for diagnostics or heavy payloads. |
| `POST /api/sync/push` | Batch push local changes to server (ordered by FK dependency) |
| `POST /api/sync/pull` | Batch pull server changes since last sync timestamp |
| `POST /api/auth/device` | Register device (validates API key, stores DeviceId) |

---

## Implementation Phases

### Phase 1 — Domain: Sync Primitives

**New files:**
- `Domain/Common/ISyncable.cs`
- `Domain/Enums/SyncStatus.cs`
- `Domain/Enums/SyncOperationType.cs`
- `Domain/Entities/SyncQueueEntry.cs`

**Modified files:**
- `Domain/Entities/Customer.cs` — add ISyncable
- `Domain/Entities/Vehicle.cs` — add ISyncable
- `Domain/Entities/MaintenanceRecord.cs` — add ISyncable
- `Domain/Entities/Inspection.cs` — add ISyncable
- `Domain/Entities/Reminder.cs` — add ISyncable

### Phase 2 — Persistence: Schema + Auto-Enqueue

**New files:**
- `Persistence/Configurations/SyncQueueEntryConfiguration.cs`

**Modified files:**
- `Persistence/Context/GarageFlowDbContext.cs` — add SyncQueue DbSet, IsSyncSuppressed flag, SaveChangesAsync interception with enqueue rules
- 5x entity configurations — add sync column config (CloudId unique index, etc.)
- `Persistence/DependencyInjection.cs` — register SyncQueueRepository

**Then:** create EF migration `AddSyncSupport`

### Phase 3 — Application: Sync Interfaces & DTOs

**New files:**
- `Application/Interfaces/ISyncService.cs`
- `Application/Interfaces/ISyncApiClient.cs`
- `Application/Interfaces/ISyncQueueRepository.cs`
- `Application/DTOs/Sync/SyncEntityDto.cs`
- `Application/DTOs/Sync/PushRequest.cs`
- `Application/DTOs/Sync/PushResponse.cs`
- `Application/DTOs/Sync/PullRequest.cs`
- `Application/DTOs/Sync/PullResponse.cs`
- `Application/Common/SyncState.cs`

### Phase 4 — Infrastructure: Sync Engine

**New files:**
- `Infrastructure/Sync/SyncConfiguration.cs`
- `Infrastructure/Sync/SyncApiClient.cs`
- `Infrastructure/Sync/SyncService.cs`
- `Infrastructure/Sync/BackgroundSyncService.cs`
- `Persistence/Repositories/SyncQueueRepository.cs`

**Modified files:**
- `Infrastructure/DependencyInjection.cs` — register sync services + HttpClientFactory
- `Infrastructure/GarageFlow.Infrastructure.csproj` — add Microsoft.Extensions.Http

### Phase 5 — WPF: .env Loading

**Modified files:**
- `Wpf/App.xaml.cs` — load .env.local via DotNetEnv
- `Wpf/GarageFlow.Wpf.csproj` — add DotNetEnv + copy .env.local

### Phase 6 — API Project: GarageFlow.Api

**New project** (ASP.NET Core Web API, net8.0):
- `GarageFlow.Api/Program.cs`
- `GarageFlow.Api/Data/ApiDbContext.cs` (PostgreSQL, internal PK + CloudId as unique sync identifier)
- `GarageFlow.Api/Data/Configurations/*.cs`
- `GarageFlow.Api/Controllers/SyncController.cs`
- `GarageFlow.Api/Controllers/AuthController.cs`
- `GarageFlow.Api/Services/SyncProcessor.cs`
- `GarageFlow.Api/Models/*.cs` (server entity mirrors)

References: Domain, Application only. NOT Persistence.

Packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `DotNetEnv`, `Serilog.AspNetCore`
Note: JWT Bearer is deferred to a later auth phase. API key auth is sufficient for MVP.

### Phase 7 — Tests

**New files:**
- `Tests/Sync/SyncQueueInterceptionTests.cs`
- `Tests/Sync/SyncServiceTests.cs`

### Phase 8 — Docker + Deployment

**New files:**
- `Dockerfile` (multi-stage .NET 8 build for API)
- `docker-compose.yml` (API + PostgreSQL, **local development only** — production uses Coolify)

---

## Dockerfile for API

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "GarageFlow.Api/GarageFlow.Api.csproj"
RUN dotnet publish "GarageFlow.Api/GarageFlow.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GarageFlow.Api.dll"]
```

---

## Deployment

| Component | Platform | Access |
|---|---|---|
| PostgreSQL | Coolify / VPS | Private (internal only) |
| GarageFlow API | Coolify / VPS | Public (api.jouwdomein.nl) |
| Desktop app | Windows PC | Local install |

---

## Key Design Decisions

| Decision | Choice | Reason |
|---|---|---|
| Sync interface | ISyncable (not BaseEntity) | Only 5 of 9 entities sync |
| CloudId | Client-generated Guid (not null) | Works offline, no post-upload mapping needed, idempotent retries |
| Queue population | SaveChangesAsync intercept | Zero service modifications needed |
| Enqueue filtering | Skip sync-metadata-only changes | Prevents noisy queue growth |
| API design | Batch push/pull | Fewer HTTP calls for desktop |
| Health vs Status | Separate /health and /api/sync/status | Different concerns: infra vs sync state |
| Conflict strategy | Optimistic concurrency, server-authoritative | Accurate description: version check + server decides |
| FK resolution | CloudId-based references in sync DTOs | Local keeps int PKs |
| Pull safety | IsSyncSuppressed flag on DbContext | Prevents re-enqueuing pulled data |
| Desktop config | No PG credentials | Security: API is the only PG gateway |
| Config location | .env.local (dev), %LocalAppData% (prod) | Proper for dev vs installed desktop |
| Auth MVP | API key only | JWT deferred to later auth phase |
| Part sync | Local-only in MVP | Becomes syncable in phase 2 if cloud needs cost detail |
| User identity | Local per device in MVP | Central identity via API in later phase |

---

## Re-entrancy Safety

When BackgroundSyncService pulls entities from the cloud and saves locally, the DbContext interception must NOT re-enqueue them. Solution: `IsSyncSuppressed` flag on DbContext, set by SyncService during pull-apply.

---

## Summary

### Keep
- Local Windows desktop app
- Local SQLite as primary database
- All existing entity relationships and soft-delete

### Add
- ISyncable interface on 5 entities (CloudId client-generated)
- SyncQueue auto-enqueue via SaveChanges (with filtering rules)
- Background sync service (push-first)
- ASP.NET Core API with PostgreSQL
- Separate /health and /api/sync/status endpoints
- Dockerfile for Coolify deployment

### Do not
- Put PostgreSQL credentials in the desktop app
- Connect desktop directly to PostgreSQL
- Convert the app into a website
- Break offline functionality
