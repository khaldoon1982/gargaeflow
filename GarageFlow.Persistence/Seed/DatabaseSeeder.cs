using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using GarageFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Persistence.Seed;

public static class DatabaseSeeder
{
    private static int _customerCounter;

    public static async Task SeedAsync(GarageFlowDbContext context, Func<string, string> hashPassword)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        // Users
        context.Users.AddRange(
            new User { Username = "admin", PasswordHash = hashPassword("admin123"), FullName = "Beheerder", Role = UserRole.Admin },
            new User { Username = "technicus", PasswordHash = hashPassword("tech123"), FullName = "Piet Bakker", Role = UserRole.Technician },
            new User { Username = "receptie", PasswordHash = hashPassword("receptie123"), FullName = "Karin Jansen", Role = UserRole.Receptionist }
        );

        // Customers - Private
        var c1 = new Customer { CustomerNumber = NextCustomerNumber(), CustomerType = CustomerType.Private, FirstName = "Mohammed", LastName = "El Amrani", PhoneNumber = "06-12345678", WhatsAppNumber = "06-12345678", Email = "m.elamrani@email.nl", Street = "Kerkstraat", HouseNumber = "12", PostalCode = "1012 AB", City = "Amsterdam", LastActivityAt = DateTime.UtcNow.AddDays(-2) };
        var c2 = new Customer { CustomerNumber = NextCustomerNumber(), CustomerType = CustomerType.Private, FirstName = "Sophie", LastName = "van den Berg", PhoneNumber = "06-87654321", Email = "s.vandenberg@email.nl", Street = "Dorpsweg", HouseNumber = "45", PostalCode = "3012 CD", City = "Rotterdam", LastActivityAt = DateTime.UtcNow.AddDays(-5) };
        var c3 = new Customer { CustomerNumber = NextCustomerNumber(), CustomerType = CustomerType.Private, FirstName = "Jan", LastName = "de Groot", PhoneNumber = "06-11223344", Email = "j.degroot@email.nl", Street = "Hoofdstraat", HouseNumber = "78", PostalCode = "2511 EG", City = "Den Haag", LastActivityAt = DateTime.UtcNow.AddDays(-1) };

        // Customers - Business
        var c4 = new Customer { CustomerNumber = NextCustomerNumber(), CustomerType = CustomerType.Business, CompanyName = "ABC Logistics B.V.", ContactPerson = "Ahmed Benali", PhoneNumber = "020-5551234", Email = "info@abclogistics.nl", BillingEmail = "factuur@abclogistics.nl", ChamberOfCommerceNumber = "12345678", VATNumber = "NL123456789B01", PaymentTerm = PaymentTerm.Days30, Street = "Industrieweg", HouseNumber = "22", PostalCode = "1043 AP", City = "Amsterdam", LastActivityAt = DateTime.UtcNow };
        var c5 = new Customer { CustomerNumber = NextCustomerNumber(), CustomerType = CustomerType.Business, CompanyName = "Bakkerij De Zon", ContactPerson = "Lisa Bakker", PhoneNumber = "010-4567890", Email = "info@bakkerijdezon.nl", PaymentTerm = PaymentTerm.Days14, Street = "Bakkersstraat", HouseNumber = "5", PostalCode = "3011 AA", City = "Rotterdam", LastActivityAt = DateTime.UtcNow.AddDays(-10) };

        context.Customers.AddRange(c1, c2, c3, c4, c5);
        await context.SaveChangesAsync();

        // Vehicles
        var v1 = new Vehicle { PlateNumberOriginal = "AB-123-CD", PlateNumberNormalized = "AB123CD", Brand = "Volkswagen", Model = "Golf", Year = 2020, FuelType = FuelType.Benzine, TransmissionType = TransmissionType.Handgeschakeld, Mileage = 45000, Color = "Zwart", VIN = "WVWZZZ1KZAW123456", InspectionExpiryDate = DateTime.UtcNow.AddMonths(1), CustomerId = c1.Id };
        var v2 = new Vehicle { PlateNumberOriginal = "EF-456-GH", PlateNumberNormalized = "EF456GH", Brand = "Toyota", Model = "Yaris", Year = 2022, FuelType = FuelType.Hybride, TransmissionType = TransmissionType.Automaat, Mileage = 20000, Color = "Wit", InspectionExpiryDate = DateTime.UtcNow.AddDays(-10), CustomerId = c2.Id };
        var v3 = new Vehicle { PlateNumberOriginal = "IJ-789-KL", PlateNumberNormalized = "IJ789KL", Brand = "BMW", Model = "3 Serie", Year = 2019, FuelType = FuelType.Diesel, TransmissionType = TransmissionType.Automaat, Mileage = 78000, Color = "Grijs", InspectionExpiryDate = DateTime.UtcNow.AddDays(5), CustomerId = c3.Id };
        // Business fleet
        var v4 = new Vehicle { PlateNumberOriginal = "MN-001-OP", PlateNumberNormalized = "MN001OP", Brand = "Mercedes", Model = "Sprinter", Year = 2021, FuelType = FuelType.Diesel, TransmissionType = TransmissionType.Handgeschakeld, Mileage = 120000, Color = "Wit", CustomerId = c4.Id };
        var v5 = new Vehicle { PlateNumberOriginal = "MN-002-OP", PlateNumberNormalized = "MN002OP", Brand = "Mercedes", Model = "Vito", Year = 2020, FuelType = FuelType.Diesel, TransmissionType = TransmissionType.Automaat, Mileage = 95000, Color = "Wit", CustomerId = c4.Id };
        var v6 = new Vehicle { PlateNumberOriginal = "QR-345-ST", PlateNumberNormalized = "QR345ST", Brand = "Renault", Model = "Kangoo", Year = 2021, FuelType = FuelType.Benzine, TransmissionType = TransmissionType.Handgeschakeld, Mileage = 45000, Color = "Blauw", CustomerId = c5.Id };
        context.Vehicles.AddRange(v1, v2, v3, v4, v5, v6);
        await context.SaveChangesAsync();

        // Maintenance records
        var m1 = new MaintenanceRecord { ServiceDate = DateTime.UtcNow.AddDays(-30), MileageAtService = 44000, ServiceType = "Grote beurt", Description = "Olie verversen, filters vervangen, remmen gecontroleerd", LaborCost = 150, PartsCost = 200, TotalCost = 350, TechnicianName = "Piet Bakker", NextServiceDate = DateTime.UtcNow.AddMonths(6), Status = MaintenanceStatus.Afgerond, VehicleId = v1.Id };
        var m2 = new MaintenanceRecord { ServiceDate = DateTime.UtcNow.AddDays(7), ServiceType = "APK voorbereiding", Description = "Voorbereiding voor APK keuring", LaborCost = 75, PartsCost = 0, TotalCost = 75, TechnicianName = "Piet Bakker", Status = MaintenanceStatus.Gepland, VehicleId = v2.Id };
        var m3 = new MaintenanceRecord { ServiceDate = DateTime.UtcNow.AddDays(-5), MileageAtService = 77500, ServiceType = "Reparatie", Description = "Distributieriem vervangen", PartsChanged = "Distributieriem, spanrol", LaborCost = 300, PartsCost = 180, TotalCost = 480, TechnicianName = "Piet Bakker", Status = MaintenanceStatus.InBehandeling, VehicleId = v3.Id };
        context.MaintenanceRecords.AddRange(m1, m2, m3);
        await context.SaveChangesAsync();

        // Parts
        context.Parts.AddRange(
            new Part { Name = "Motorolie 5W-30", PartNumber = "OIL-5W30-5L", Quantity = 1, UnitPrice = 45, TotalPrice = 45, MaintenanceRecordId = m1.Id },
            new Part { Name = "Oliefilter", PartNumber = "FLT-OIL-VW01", Quantity = 1, UnitPrice = 15, TotalPrice = 15, MaintenanceRecordId = m1.Id },
            new Part { Name = "Distributieriem set", PartNumber = "BLT-DIST-BMW01", Quantity = 1, UnitPrice = 140, TotalPrice = 140, MaintenanceRecordId = m3.Id }
        );

        // Inspections
        context.Inspections.AddRange(
            new Inspection { InspectionType = InspectionType.APK, InspectionDate = DateTime.UtcNow.AddMonths(-11), ExpiryDate = DateTime.UtcNow.AddMonths(1), Status = InspectionStatus.Goedgekeurd, VehicleId = v1.Id },
            new Inspection { InspectionType = InspectionType.APK, InspectionDate = DateTime.UtcNow.AddMonths(-13), ExpiryDate = DateTime.UtcNow.AddDays(-10), Status = InspectionStatus.Verlopen, VehicleId = v2.Id },
            new Inspection { InspectionType = InspectionType.APK, InspectionDate = DateTime.UtcNow.AddMonths(-6), ExpiryDate = DateTime.UtcNow.AddDays(5), Status = InspectionStatus.Goedgekeurd, VehicleId = v3.Id }
        );

        // Reminders
        context.Reminders.AddRange(
            new Reminder { ReminderType = ReminderType.APKVerlopen, ReminderDate = DateTime.UtcNow.AddDays(-10), Message = "APK keuring verlopen voor EF-456-GH", Status = ReminderStatus.Openstaand, CustomerId = c2.Id, VehicleId = v2.Id },
            new Reminder { ReminderType = ReminderType.KeuringBinnenkort, ReminderDate = DateTime.UtcNow, Message = "APK keuring verloopt binnenkort voor IJ-789-KL", Status = ReminderStatus.Openstaand, CustomerId = c3.Id, VehicleId = v3.Id }
        );

        // App settings
        context.AppSettings.AddRange(
            new AppSetting { Key = "WorkshopName", Value = "GarageFlow Auto Service" },
            new AppSetting { Key = "WorkshopPhone", Value = "020-1234567" },
            new AppSetting { Key = "WorkshopAddress", Value = "Industrieweg 10, 1000 AA Amsterdam" },
            new AppSetting { Key = "LogoPath", Value = "" },
            new AppSetting { Key = "ReminderLeadDays", Value = "7" },
            new AppSetting { Key = "BackupFolder", Value = "backups" },
            new AppSetting { Key = "NextCustomerNumber", Value = "6" }
        );

        await context.SaveChangesAsync();
    }

    private static string NextCustomerNumber()
    {
        _customerCounter++;
        return $"KL-{_customerCounter:D6}";
    }
}
