using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.PlateNumberOriginal).IsRequired().HasMaxLength(20);
        builder.Property(v => v.PlateNumberNormalized).IsRequired().HasMaxLength(20);
        builder.Property(v => v.Brand).IsRequired().HasMaxLength(50);
        builder.Property(v => v.Model).IsRequired().HasMaxLength(50);
        builder.Property(v => v.Trim).HasMaxLength(50);
        builder.Property(v => v.ChassisNumber).HasMaxLength(50);
        builder.Property(v => v.EngineNumber).HasMaxLength(50);
        builder.Property(v => v.VIN).HasMaxLength(17);
        builder.Property(v => v.Color).HasMaxLength(30);
        builder.Property(v => v.Notes).HasMaxLength(2000);
        builder.HasIndex(v => v.PlateNumberNormalized);
        builder.HasIndex(v => v.ChassisNumber);
        builder.ConfigureSyncFields();
        builder.HasQueryFilter(v => v.IsActive);
        builder.HasMany(v => v.MaintenanceRecords).WithOne(m => m.Vehicle).HasForeignKey(m => m.VehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.Inspections).WithOne(i => i.Vehicle).HasForeignKey(i => i.VehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.Reminders).WithOne(r => r.Vehicle).HasForeignKey(r => r.VehicleId).OnDelete(DeleteBehavior.Restrict);
    }
}
