using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PartNumber).HasMaxLength(50);
        builder.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalPrice).HasColumnType("decimal(18,2)");
        builder.HasQueryFilter(p => p.IsActive);
    }
}
