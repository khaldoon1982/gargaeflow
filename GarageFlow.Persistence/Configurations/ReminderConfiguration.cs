using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Message).IsRequired().HasMaxLength(1000);
        builder.HasIndex(r => r.ReminderDate);
        builder.ConfigureSyncFields();
        builder.HasQueryFilter(r => r.IsActive);
    }
}
