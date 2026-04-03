# GarageFlow – Database Technische Documentatie

## 1. Database Technologie

| Eigenschap | Waarde |
|---|---|
| **Engine** | SQLite |
| **EF Core Provider** | `Microsoft.EntityFrameworkCore.Sqlite` v8.0.11 |
| **Bestandslocatie** | `{AppBaseDirectory}/garageflow.db` |
| **Connection string** | `Data Source={pad naar .db bestand}` |
| **Benadering** | Code-First met EF Core Migrations |

SQLite is een embedded database — geen aparte server nodig. Het hele database bestaat uit één `.db` bestand op schijf.

---

## 2. Architectuur Overzicht

```
GarageFlow.Domain          ← Entities, Enums, BaseEntity, ISoftDeletable
GarageFlow.Application     ← IRepository<T>, Services, DTOs, Validators
GarageFlow.Persistence     ← DbContext, Configurations, Repository<T>, Migrations, Seed
GarageFlow.Infrastructure  ← BackupService, PasswordHasher, PlateNormalizationService
GarageFlow.Wpf             ← App.xaml.cs (host + DI + startup)
```

**Dependency flow**: Domain ← Application ← Persistence ← Infrastructure ← Wpf

---

## 3. DbContext

**Bestand**: `GarageFlow.Persistence/Context/GarageFlowDbContext.cs`

### DbSets (9 tabellen)

| DbSet | Entity | Beschrijving |
|---|---|---|
| `Users` | User | Gebruikers met rollen en wachtwoord-hash |
| `Customers` | Customer | Klanten (particulier + zakelijk) |
| `Vehicles` | Vehicle | Voertuigen gekoppeld aan klanten |
| `MaintenanceRecords` | MaintenanceRecord | Onderhoudsregistraties |
| `Inspections` | Inspection | APK en keuringen |
| `Reminders` | Reminder | Herinneringen |
| `Parts` | Part | Onderdelen bij onderhoud |
| `AuditLogs` | AuditLog | Audit trail |
| `AppSettings` | AppSetting | Applicatie-instellingen (key-value) |

### Automatische timestamp tracking

```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
    {
        if (entry.State == EntityState.Modified)
            entry.Entity.UpdatedAt = DateTime.UtcNow;
    }
    return base.SaveChangesAsync(cancellationToken);
}
```

Elke entiteit die `BaseEntity` erft krijgt automatisch `CreatedAt` en `UpdatedAt` (UTC).

---

## 4. Entity Model

### BaseEntity (abstract)

```csharp
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### ISoftDeletable

```csharp
public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
```

### Relatiediagram

```
User (1) ──── (*) AuditLog

Customer (1) ──── (*) Vehicle
Customer (1) ──── (*) Reminder

Vehicle (1) ──── (*) MaintenanceRecord
Vehicle (1) ──── (*) Inspection
Vehicle (1) ──── (*) Reminder

MaintenanceRecord (1) ──── (*) Part

AppSetting (standalone key-value tabel)
```

### Alle entiteiten en hun velden

#### Customer
- Id, CustomerNumber (unique), CustomerType (Private/Business)
- FirstName, LastName (particulier)
- CompanyName, ContactPerson, Department (zakelijk)
- PhoneNumber, WhatsAppNumber, Email, BillingEmail
- PreferredContactMethod, ChamberOfCommerceNumber, VATNumber
- PaymentTerm, DebtorNumber, FleetManager, BillingNotes
- Street, HouseNumber, HouseNumberAddition, PostalCode, City, Country
- Notes, IsArchived, LastActivityAt, IsActive
- **Computed**: DisplayName (niet in DB, `[Ignore]`)

#### Vehicle
- Id, PlateNumberOriginal, PlateNumberNormalized (index)
- Brand, Model, Trim, Year, ChassisNumber (index), EngineNumber, VIN
- FuelType, TransmissionType, Color, Mileage
- InspectionDate, InspectionExpiryDate, LastServiceDate, NextServiceDate
- Notes, IsArchived, IsActive, CustomerId (FK)

#### MaintenanceRecord
- Id, ServiceDate (index), MileageAtService, ServiceType, Description
- PartsChanged, LaborCost, PartsCost, TotalCost (decimal 18,2)
- TechnicianName, NextServiceDate, NextServiceMileage
- Status (Gepland/InBehandeling/Afgerond/Geannuleerd), Notes, IsActive, VehicleId (FK)

#### Inspection
- Id, InspectionType, InspectionDate, ExpiryDate (index), ReminderDate
- Status (Gepland/Goedgekeurd/Afgekeurd/Verlopen), Notes, IsActive, VehicleId (FK)

#### Reminder
- Id, ReminderType, ReminderDate (index), Message, SendMethod
- IsSent, SentAt, Status (Openstaand/Verzonden/Geannuleerd)
- IsActive, CustomerId (FK), VehicleId (FK, optional)

#### Part
- Id, Name, PartNumber, Quantity, UnitPrice, TotalPrice (decimal 18,2)
- IsActive, MaintenanceRecordId (FK, cascade delete)

#### User
- Id, Username (unique), PasswordHash, FullName, Role, IsActive

#### AuditLog
- Id, ActionType, EntityName, EntityId, Details, UserId (FK, nullable, SetNull)

#### AppSetting
- Id, Key (unique), Value, UpdatedAt
- **Let op**: erft NIET van BaseEntity

---

## 5. Entity Configurations

**Locatie**: `GarageFlow.Persistence/Configurations/`

Elke entiteit heeft een eigen `IEntityTypeConfiguration<T>` klasse die:
- Primary keys definieert
- Veldlengtes en constraints instelt
- Indexen aanmaakt
- Relaties configureert (FK, OnDelete behavior)
- **Query filters toepast**: `builder.HasQueryFilter(e => e.IsActive);`

### Delete Behavior

| Relatie | OnDelete |
|---|---|
| Customer → Vehicles | Restrict |
| Customer → Reminders | Restrict |
| Vehicle → MaintenanceRecords | Restrict |
| Vehicle → Inspections | Restrict |
| Vehicle → Reminders | Restrict |
| MaintenanceRecord → Parts | **Cascade** |
| User → AuditLogs | SetNull |

---

## 6. Soft Delete (Query Filters)

Alle entiteiten met `ISoftDeletable` hebben een globaal query filter:

```csharp
builder.HasQueryFilter(e => e.IsActive);
```

**Gevolg**: `_dbSet.ToListAsync()` retourneert automatisch alleen actieve records.

Om ook inactieve records op te halen:
```csharp
context.Customers.IgnoreQueryFilters().ToListAsync();
```

**Soft delete in services**:
```csharp
entity.IsActive = false;
await _repo.UpdateAsync(entity);
await _repo.SaveChangesAsync();
```

---

## 7. Repository Pattern

**Interface**: `GarageFlow.Application/Interfaces/IRepository.cs`
**Implementatie**: `GarageFlow.Persistence/Repositories/Repository.cs`

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

**Registratie** (open generic):
```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

Dit betekent dat `IRepository<Customer>`, `IRepository<Vehicle>`, etc. automatisch worden geresolved.

---

## 8. Migrations

**Locatie**: `GarageFlow.Persistence/Migrations/`

### Aanmaken nieuwe migration
```bash
dotnet ef migrations add <Naam> --project GarageFlow.Persistence --startup-project GarageFlow.Wpf
```

### Toepassen bij startup
Migrations worden automatisch toegepast bij app-start via `DatabaseSeeder.SeedAsync()`:
```csharp
await context.Database.MigrateAsync();
```

### Huidige migration
- `FullRedesign` — Bevat het volledige schema met alle 9 tabellen

---

## 9. Database Seeding

**Bestand**: `GarageFlow.Persistence/Seed/DatabaseSeeder.cs`

### Volgorde
1. `context.Database.MigrateAsync()` — schema toepassen
2. Check of database leeg is: `if (await context.Users.AnyAsync()) return;`
3. Seed data in volgorde: Users → Customers → Vehicles → MaintenanceRecords → Parts → Inspections → Reminders → AppSettings

### Standaard gebruikers

| Gebruiker | Wachtwoord | Rol |
|---|---|---|
| admin | admin123 | Admin |
| technicus | tech123 | Technician |
| receptie | receptie123 | Receptionist |

### Standaard instellingen (AppSettings)

| Key | Standaardwaarde |
|---|---|
| WorkshopName | GarageFlow Auto Service |
| WorkshopPhone | 020-1234567 |
| WorkshopAddress | Industrieweg 10, 1000 AA Amsterdam |
| LogoPath | (leeg) |
| ReminderLeadDays | 7 |
| BackupFolder | backups |
| NextCustomerNumber | 6 |

---

## 10. Backup & Restore

**Bestand**: `GarageFlow.Infrastructure/Services/BackupService.cs`

### Backup aanmaken
```
Bron: {AppBaseDirectory}/garageflow.db
Doel: {BackupFolder}/garageflow_backup_yyyyMMdd_HHmmss.db
```
Kopieert het volledige `.db` bestand.

### Restore
```
Bron: geselecteerd backup bestand
Doel: {AppBaseDirectory}/garageflow.db (overschrijft!)
```

### Beperkingen huidige aanpak
- Backup is een simpele file copy (geen transactionele backup)
- Restore vereist herstart van de applicatie
- Geen automatische backup scheduling
- Geen compressie
- Geen verificatie van backup integriteit

---

## 11. DI Registratie (volledige keten)

```csharp
// App.xaml.cs
services.AddApplication();                          // Services, Validators
services.AddPersistence($"Data Source={DbPath}");   // DbContext, Repository<T>
services.AddInfrastructure(DbPath);                 // Backup, PasswordHasher, PlateService

// Persistence DI
services.AddDbContext<GarageFlowDbContext>(options => options.UseSqlite(connectionString));
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<DbContext>(sp => sp.GetRequiredService<GarageFlowDbContext>());

// Infrastructure DI
services.AddSingleton<IPlateNormalizationService, PlateNormalizationService>();
services.AddSingleton<IPasswordHasher, PasswordHasher>();
services.AddSingleton<IBackupService>(sp => new BackupService(dbPath, ...));
```

---

## 12. Enums (domeinwaarden)

| Enum | Waarden | Locatie |
|---|---|---|
| CustomerType | Private, Business | `Domain/Enums/` |
| UserRole | Admin, Technician, Receptionist, Viewer | `Domain/Enums/` |
| FuelType | Benzine, Diesel, LPG, Elektrisch, Hybride, CNG, Overig | `Domain/Enums/` |
| TransmissionType | Handgeschakeld, Automaat, SemiAutomaat, Overig | `Domain/Enums/` |
| MaintenanceStatus | Gepland, InBehandeling, Afgerond, Geannuleerd | `Domain/Enums/` |
| InspectionType | APK, Emissietest, Veiligheidskeuring, Overig | `Domain/Enums/` |
| InspectionStatus | Gepland, Goedgekeurd, Afgekeurd, Verlopen | `Domain/Enums/` |
| PaymentTerm | Cash, Days7, Days14, Days30, Custom | `Domain/Enums/` |
| PreferredContactMethod | Phone, WhatsApp, Email | `Domain/Enums/` |
| ReminderType | APKVerlopen, OnderhoudGepland, AlgemeneHerinnering, KeuringBinnenkort | `Domain/Enums/` |
| ReminderStatus | Openstaand, Verzonden, Geannuleerd | `Domain/Enums/` |
| SendMethod | Intern, Email, WhatsApp, SMS | `Domain/Enums/` |

---

## 13. NuGet Packages (database-gerelateerd)

| Package | Versie | Project |
|---|---|---|
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.11 | Persistence |
| Microsoft.EntityFrameworkCore.Design | 8.0.11 | Persistence |
| Microsoft.EntityFrameworkCore | 8.0.11 | Application |
| Microsoft.EntityFrameworkCore.InMemory | 8.0.11 | Tests |

---

## 14. Ontwikkelcommando's

```bash
# Build
dotnet build GarageFlow.sln

# Run
dotnet run --project GarageFlow.Wpf

# Tests
dotnet test

# Nieuwe migration
dotnet ef migrations add <Naam> --project GarageFlow.Persistence --startup-project GarageFlow.Wpf

# Database verwijderen en opnieuw aanmaken
# Verwijder garageflow.db bestanden, start app opnieuw → seeder draait automatisch
```
