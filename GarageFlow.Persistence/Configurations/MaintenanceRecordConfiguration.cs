using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ServiceType).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.PartsChanged).HasMaxLength(1000);
        builder.Property(m => m.TechnicianName).HasMaxLength(200);
        builder.Property(m => m.Notes).HasMaxLength(2000);
        builder.Property(m => m.LaborCost).HasColumnType("decimal(18,2)");
        builder.Property(m => m.PartsCost).HasColumnType("decimal(18,2)");
        builder.Property(m => m.TotalCost).HasColumnType("decimal(18,2)");
        builder.HasIndex(m => m.ServiceDate);
        builder.ConfigureSyncFields();
        builder.HasQueryFilter(m => m.IsActive);
        builder.HasMany(m => m.Parts).WithOne(p => p.MaintenanceRecord).HasForeignKey(p => p.MaintenanceRecordId).OnDelete(DeleteBehavior.Cascade);
    }
}
