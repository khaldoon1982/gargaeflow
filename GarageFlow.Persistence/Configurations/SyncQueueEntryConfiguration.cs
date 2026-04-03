using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class SyncQueueEntryConfiguration : IEntityTypeConfiguration<SyncQueueEntry>
{
    public void Configure(EntityTypeBuilder<SyncQueueEntry> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(s => new { s.Status, s.CreatedAtUtc });
        builder.HasIndex(s => new { s.EntityName, s.EntityId });
    }
}
