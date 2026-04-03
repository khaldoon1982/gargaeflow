using GarageFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Api.Data.Configurations;

public class CloudCustomerConfig : IEntityTypeConfiguration<CloudCustomer>
{
    public void Configure(EntityTypeBuilder<CloudCustomer> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.CloudId).IsUnique();
        b.HasIndex(e => e.DeviceId);
        b.HasIndex(e => e.UpdatedAtUtc);
    }
}

public class CloudVehicleConfig : IEntityTypeConfiguration<CloudVehicle>
{
    public void Configure(EntityTypeBuilder<CloudVehicle> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.CloudId).IsUnique();
        b.HasIndex(e => e.CustomerCloudId);
        b.HasIndex(e => e.UpdatedAtUtc);
    }
}

public class CloudMaintenanceRecordConfig : IEntityTypeConfiguration<CloudMaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<CloudMaintenanceRecord> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.CloudId).IsUnique();
        b.HasIndex(e => e.VehicleCloudId);
        b.HasIndex(e => e.UpdatedAtUtc);
    }
}

public class CloudInspectionConfig : IEntityTypeConfiguration<CloudInspection>
{
    public void Configure(EntityTypeBuilder<CloudInspection> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.CloudId).IsUnique();
        b.HasIndex(e => e.VehicleCloudId);
        b.HasIndex(e => e.UpdatedAtUtc);
    }
}

public class CloudReminderConfig : IEntityTypeConfiguration<CloudReminder>
{
    public void Configure(EntityTypeBuilder<CloudReminder> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.CloudId).IsUnique();
        b.HasIndex(e => e.CustomerCloudId);
        b.HasIndex(e => e.UpdatedAtUtc);
    }
}

public class DeviceRegistrationConfig : IEntityTypeConfiguration<DeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DeviceRegistration> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.DeviceId).IsUnique();
        b.Property(e => e.DeviceId).IsRequired().HasMaxLength(50);
    }
}

public class SyncLogConfig : IEntityTypeConfiguration<SyncLog>
{
    public void Configure(EntityTypeBuilder<SyncLog> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.TimestampUtc);
        b.Property(e => e.DeviceId).IsRequired().HasMaxLength(50);
        b.Property(e => e.Direction).IsRequired().HasMaxLength(10);
    }
}
