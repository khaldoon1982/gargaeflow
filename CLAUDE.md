# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GarageFlow (MotoLedger) is a WPF desktop application for auto garage management built with .NET 8 and C#. The domain uses Dutch-language enum values (e.g., `Benzine`, `Handgeschakeld`, `Gepland`).

## Build & Run Commands

```bash
# Build entire solution
dotnet build GarageFlow.sln

# Run the WPF app (Windows only)
dotnet run --project GarageFlow.Wpf

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~ClassName"

# Add EF Core migration
dotnet ef migrations add <Name> --project GarageFlow.Persistence --startup-project GarageFlow.Wpf
```

## Architecture

Clean Architecture with strict dependency flow: **Domain ← Application ← Infrastructure/Persistence ← Wpf**

| Layer | Purpose | Key Dependencies |
|---|---|---|
| **GarageFlow.Domain** | Entities, enums, `BaseEntity`, `ISoftDeletable` | None |
| **GarageFlow.Application** | DTOs, interfaces, services, validators | Domain, FluentValidation, Serilog |
| **GarageFlow.Persistence** | EF Core DbContext, configurations, repositories, migrations, seed data | Domain, Application, EF Core SQLite |
| **GarageFlow.Infrastructure** | External service implementations | Domain, Application, Persistence, Serilog |
| **GarageFlow.Wpf** | WPF UI (Views, ViewModels, Controls, Converters) | Application, Infrastructure, Persistence, CommunityToolkit.Mvvm, MS DI/Hosting |
| **GarageFlow.Tests** | xUnit tests for services and validators | All layers, EF Core InMemory |

## Key Patterns

- **MVVM** in the WPF layer using CommunityToolkit.Mvvm (`ObservableObject`, `RelayCommand`)
- **Soft delete** via `ISoftDeletable` interface (`IsActive` flag) — entities are deactivated, not removed
- **BaseEntity** provides `CreatedAt`/`UpdatedAt` audit timestamps (UTC)
- **FluentValidation** for input validation in the Application layer
- **Serilog** for structured logging (file + console sinks)
- **SQLite** as the database via EF Core
- **DI** via Microsoft.Extensions.DependencyInjection with Generic Host
- **User roles**: Admin, Technician, Receptionist, Viewer
