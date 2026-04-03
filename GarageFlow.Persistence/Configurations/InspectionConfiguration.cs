using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.HasIndex(i => i.ExpiryDate);
        builder.ConfigureSyncFields();
        builder.HasQueryFilter(i => i.IsActive);
    }
}
