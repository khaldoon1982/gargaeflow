using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CustomerType = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BillingEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ChamberOfCommerceNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    VATNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PaymentTerm = table.Column<int>(type: "INTEGER", nullable: true),
                    DebtorNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FleetManager = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BillingNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PreferredContactMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Street = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    HouseNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    HouseNumberAddition = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CloudId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLocalChangeAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncQueue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    OperationType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ProcessedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlateNumberOriginal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PlateNumberNormalized = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Trim = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    ChassisNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EngineNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VIN = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    FuelType = table.Column<int>(type: "INTEGER", nullable: false),
                    TransmissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Mileage = table.Column<int>(type: "INTEGER", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InspectionExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CloudId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLocalChangeAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InspectionType = table.Column<int>(type: "INTEGER", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CloudId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLocalChangeAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MileageAtService = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PartsChanged = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TechnicianName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextServiceMileage = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CloudId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLocalChangeAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReminderType = table.Column<int>(type: "INTEGER", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SendMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CloudId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLocalChangeAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reminders_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaintenanceRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_MaintenanceRecords_MaintenanceRecordId",
                        column: x => x.MaintenanceRecordId,
                        principalTable: "MaintenanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CloudId",
                table: "Customers",
                column: "CloudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerNumber",
                table: "Customers",
                column: "CustomerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SyncStatus",
                table: "Customers",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_CloudId",
                table: "Inspections",
                column: "CloudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ExpiryDate",
                table: "Inspections",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_SyncStatus",
                table: "Inspections",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_VehicleId",
                table: "Inspections",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_CloudId",
                table: "MaintenanceRecords",
                column: "CloudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_ServiceDate",
                table: "MaintenanceRecords",
                column: "ServiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_SyncStatus",
                table: "MaintenanceRecords",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_VehicleId",
                table: "MaintenanceRecords",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_MaintenanceRecordId",
                table: "Parts",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_CloudId",
                table: "Reminders",
                column: "CloudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_CustomerId",
                table: "Reminders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_ReminderDate",
                table: "Reminders",
                column: "ReminderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_SyncStatus",
                table: "Reminders",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_VehicleId",
                table: "Reminders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueue_EntityName_EntityId",
                table: "SyncQueue",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueue_Status_CreatedAtUtc",
                table: "SyncQueue",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ChassisNumber",
                table: "Vehicles",
                column: "ChassisNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CloudId",
                table: "Vehicles",
                column: "CloudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PlateNumberNormalized",
                table: "Vehicles",
                column: "PlateNumberNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SyncStatus",
                table: "Vehicles",
                column: "SyncStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "SyncQueue");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
