You are a senior software architect and senior full-stack Windows desktop engineer.

Your task is to design and build a production-structured Windows desktop application called **GarageFlow** for managing a car garage/workshop.

IMPORTANT LANGUAGE REQUIREMENT:
- The entire application UI must be in **Dutch (NL)**.
- All labels, buttons, menus, form titles, dashboard widgets, validation messages, status texts, and notifications shown to end users must be in Dutch.
- Code, classes, database table names, entity names, services, comments, architecture folders, and internal developer documentation should remain in **English** for maintainability.
- Use a localization-friendly approach so Arabic and English can be added later, but implement Dutch now as the default language.

--------------------------------------------------
1. PRODUCT GOAL
--------------------------------------------------

Build a professional Windows desktop application for a garage/workshop that can:

- register customers
- register vehicles
- store plate numbers
- search quickly by plate number
- track maintenance/service history
- track inspection / APK / periodic due dates
- generate reminders
- show dashboard with upcoming deadlines
- filter records efficiently
- support backup and restore
- be extensible for future WhatsApp/SMS/email integration

The app is intended for real workshop operations, with fast data entry and practical day-to-day use.

--------------------------------------------------
2. REQUIRED TECH STACK
--------------------------------------------------

Use this stack unless there is a very strong technical reason not to:

- .NET 8
- C#
- WPF
- MVVM
- Entity Framework Core
- SQLite
- Serilog for logging
- FluentValidation if useful
- Clean Architecture or well-structured Layered Architecture
- Dependency Injection
- LiveCharts2 or equivalent for dashboard charts
- QuestPDF or equivalent for PDF exports if reports are implemented
- Async programming where appropriate

Do NOT use a web-based Electron stack.
This must be a true Windows desktop application.

--------------------------------------------------
3. ARCHITECTURE REQUIREMENTS
--------------------------------------------------

Use a maintainable project structure with clear separation of concerns.

Preferred solution structure:

- GarageFlow.Domain
- GarageFlow.Application
- GarageFlow.Infrastructure
- GarageFlow.Persistence
- GarageFlow.Wpf

If needed, combine Persistence into Infrastructure, but keep boundaries clean.

Use:
- Domain entities
- DTOs / view models
- Services
- Repositories or EF-based abstractions where justified
- Commands / queries or clear use-case services
- Validation layer
- Logging
- Error handling
- Seed/demo data support

Avoid putting business logic directly in XAML code-behind.

--------------------------------------------------
4. DATABASE REQUIREMENTS
--------------------------------------------------

Use SQLite for the initial implementation.

Design a clean relational schema.

Required tables/entities:

1. Users
- UserId
- Username
- PasswordHash
- FullName
- Role
- IsActive
- CreatedAt
- UpdatedAt

2. Customers
- CustomerId
- FullName
- PhoneNumber
- WhatsAppNumber
- Email
- Address
- Notes
- IsActive
- CreatedAt
- UpdatedAt

3. Vehicles
- VehicleId
- CustomerId
- PlateNumberOriginal
- PlateNumberNormalized
- ChassisNumber
- EngineNumber
- VIN
- Brand
- Model
- Year
- Color
- FuelType
- TransmissionType
- Mileage
- Notes
- IsActive
- CreatedAt
- UpdatedAt

4. MaintenanceRecords
- MaintenanceId
- VehicleId
- ServiceDate
- MileageAtService
- ServiceType
- Description
- PartsChanged
- LaborCost
- PartsCost
- TotalCost
- TechnicianName
- NextServiceDate
- NextServiceMileage
- Status
- Notes
- CreatedAt
- UpdatedAt

5. Inspections
- InspectionId
- VehicleId
- InspectionType
- InspectionDate
- ExpiryDate
- ReminderDate
- Status
- Notes
- CreatedAt
- UpdatedAt

6. Reminders
- ReminderId
- CustomerId
- VehicleId
- ReminderType
- ReminderDate
- Message
- SendMethod
- IsSent
- SentAt
- Status
- CreatedAt
- UpdatedAt

7. AuditLogs
- AuditLogId
- UserId
- ActionType
- EntityName
- EntityId
- Details
- CreatedAt

8. AppSettings
- SettingId
- Key
- Value
- UpdatedAt

Add indexes on:
- PlateNumberNormalized
- PhoneNumber
- ChassisNumber
- ServiceDate
- ExpiryDate
- ReminderDate

Use EF Core migrations.

--------------------------------------------------
5. PLATE NUMBER SEARCH REQUIREMENT
--------------------------------------------------

This is one of the most important features.

Implement:
- PlateNumberOriginal
- PlateNumberNormalized

Normalization rules:
- remove spaces
- remove dashes
- uppercase all letters
- trim surrounding whitespace

Examples:
- "12-ab-34" -> "12AB34"
- " 99-KL-7 " -> "99KL7"

Search must work using normalized plate values for speed and consistency.

Provide a dedicated service such as:
- IPlateNormalizationService
- PlateNormalizationService

Use it consistently during create, update, and search.

--------------------------------------------------
6. UI LANGUAGE REQUIREMENT (DUTCH)
--------------------------------------------------

All user-facing UI must be in Dutch.

Examples:
- Dashboard -> Dashboard
- Customers -> Klanten
- Vehicles -> Voertuigen
- Maintenance -> Onderhoud
- Inspections -> Keuringen
- Reminders -> Herinneringen
- Settings -> Instellingen
- Backup -> Back-up
- Save -> Opslaan
- Cancel -> Annuleren
- Search -> Zoeken
- Add Customer -> Klant toevoegen
- Add Vehicle -> Voertuig toevoegen
- Plate Number -> Kenteken
- Chassis Number -> Chassisnummer
- Upcoming Inspections -> Aankomende keuringen
- Overdue -> Verlopen
- Due Soon -> Binnenkort vervalt
- Service History -> Onderhoudsgeschiedenis

Validation messages must also be in Dutch, such as:
- "Naam is verplicht."
- "Kenteken is verplicht."
- "Ongeldig e-mailadres."
- "Datum is verplicht."

Keep all internal code in English.

--------------------------------------------------
7. REQUIRED MODULES
--------------------------------------------------

Implement these modules:

A. Authentication
- Login screen
- Username/password
- Password hashing
- Role-based access
- Logout

B. Dashboard
Show:
- totaal aantal klanten
- totaal aantal voertuigen
- onderhoud deze maand
- keuringen binnen 7 dagen
- verlopen keuringen
- openstaande herinneringen
- recente onderhoudsregistraties

C. Customer Management
- create
- edit
- view
- search
- list
- safe delete behavior

D. Vehicle Management
- create
- edit
- view
- search
- list
- link to customer
- show full vehicle profile
- show maintenance and inspection history

E. Maintenance Records
- create
- edit
- view history
- statuses
- costs
- next service date / mileage

F. Inspection Tracking
- track dates
- expiry dates
- statuses
- due soon / overdue logic

G. Reminder Management
- internal reminders only for MVP
- reminder queue
- manual mark as sent
- prevent duplicates

H. Search and Filtering
- global search
- search by kenteken
- filter by status
- filter by date
- filter by brand
- filter by customer
- filter by overdue/due soon

I. Reports / Export
If time permits in MVP:
- PDF or CSV export
- vehicle history report
- upcoming inspections report
- overdue report

J. Backup / Restore
- one-click backup
- restore support
- timestamped backup file
- log backup events

K. Settings
- workshop name
- logo path if needed
- phone
- address
- reminder lead days
- backup folder

--------------------------------------------------
8. BUSINESS RULES
--------------------------------------------------

Implement these rules:

- A customer can own multiple vehicles.
- A vehicle belongs to one customer.
- A vehicle must have a kenteken (plate number).
- Kenteken search must use normalized form.
- Chassis number should be unique if provided.
- Costs cannot be negative.
- TotalCost = LaborCost + PartsCost if auto-calculated.
- If inspection expiry date is past and not completed, status becomes expired/verlopen.
- If an inspection is due within configured days and no reminder exists, create one.
- Prevent duplicate active reminders for the same event.
- Deleting records should be safe; prefer soft delete or prevent deletion when linked records exist.
- Store CreatedAt / UpdatedAt consistently.

--------------------------------------------------
9. DESIGN / UX REQUIREMENTS
--------------------------------------------------

The app should look modern, clean, and practical.

Requirements:
- professional business-style layout
- left sidebar navigation or top navigation with clean sections
- dashboard cards
- data grids for lists
- clear forms for add/edit
- keyboard-friendly data entry
- readable typography
- status badges
- confirmation dialogs for destructive actions
- empty states
- loading states where relevant

Use a neutral professional color palette.
Do not overdesign.
Prioritize usability and speed.

Main screens:
- Login
- Dashboard
- Klanten
- Klantdetails
- Voertuigen
- Voertuigdetails
- Onderhoud
- Keuringen
- Herinneringen
- Rapporten
- Back-up / Herstellen
- Instellingen
- Gebruikersbeheer

--------------------------------------------------
10. DEVELOPMENT PHASES
--------------------------------------------------

Build in this order.

Phase 1:
- solution structure
- architecture setup
- EF Core DbContext
- entities
- migrations
- seed data
- login screen
- base shell/navigation

Phase 2:
- customer CRUD
- vehicle CRUD
- plate normalization service
- vehicle search by kenteken
- customer-vehicle linking

Phase 3:
- maintenance module
- inspections module
- vehicle profile page
- due-date calculations

Phase 4:
- dashboard implementation
- reminder engine
- reminder list and status updates
- filtering and search optimization

Phase 5:
- backup/restore
- logging
- validation improvements
- audit logs
- exports/reports if feasible

--------------------------------------------------
11. MVP DEFINITION
--------------------------------------------------

The MVP must include:

- login
- dashboard
- customers CRUD
- vehicles CRUD
- search by kenteken
- maintenance records
- inspection tracking
- reminders list
- backup

Do not delay MVP for SMS/WhatsApp/email integrations.

--------------------------------------------------
12. FUTURE-READY DESIGN
--------------------------------------------------

Design the code so the following can be added later:
- WhatsApp reminders
- SMS reminders
- email reminders
- Arabic UI
- English UI
- invoice generation
- spare parts inventory
- multi-branch support
- web portal
- cloud sync
- mobile app

--------------------------------------------------
13. CODE QUALITY REQUIREMENTS
--------------------------------------------------

You must produce:
- clean, readable, production-style code
- consistent naming
- good folder structure
- no dead code
- no business logic in UI code-behind
- comments only where useful
- proper error handling
- async usage where appropriate
- reusable controls/services where useful
- strongly typed models
- migrations and startup instructions

Where possible, also add:
- unit tests for core services
- especially plate normalization and reminder logic

--------------------------------------------------
14. DELIVERABLES
--------------------------------------------------

Generate the following:

1. Architecture overview
2. Folder/project structure
3. Database schema
4. EF Core entities
5. DbContext
6. Migrations
7. Seed data
8. Core services
9. WPF shell and navigation
10. Dutch UI screens
11. CRUD workflows
12. Dashboard
13. Reminder engine
14. Backup/restore
15. Setup instructions
16. Notes for future localization

--------------------------------------------------
15. WORKING METHOD
--------------------------------------------------

Work incrementally.
Do not dump everything at once.

For each major step:
1. explain what you are building
2. generate the code
3. show file structure changes
4. keep consistency with previous files
5. avoid placeholders unless absolutely necessary

If you must make assumptions, choose sensible defaults and continue.

Do not keep asking unnecessary questions.
Make strong practical engineering decisions and proceed.

--------------------------------------------------
16. FIRST TASKS TO EXECUTE NOW
--------------------------------------------------

Start immediately with these tasks:

1. Propose the final solution/project folder structure
2. Define the domain entities and enums
3. Define the EF Core DbContext
4. Configure SQLite
5. Create initial migration
6. Implement plate normalization service
7. Create base WPF shell with Dutch navigation
8. Implement login screen
9. Implement customer CRUD
10. Implement vehicle CRUD
11. Implement fast kenteken search
12. Prepare dashboard skeleton

After that, continue module by module until the MVP is complete.

--------------------------------------------------
17. OUTPUT FORMAT
--------------------------------------------------

When replying:
- be structured
- show created files clearly
- provide code in complete files when possible
- keep the app UI in Dutch
- keep code and architecture in English
- mention any package dependencies added
- mention migration commands if needed

Build this as a serious production-ready MVP for a real garage business.