using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public static class SyncableEntityExtensions
{
    public static void ConfigureSyncFields<T>(this EntityTypeBuilder<T> builder) where T : class, ISyncable
    {
        builder.Property(e => e.CloudId).IsRequired();
        builder.HasIndex(e => e.CloudId).IsUnique();
        builder.Property(e => e.SyncStatus).HasDefaultValue(SyncStatus.PendingUpload);
        builder.Property(e => e.VersionNumber).HasDefaultValue(1);
        builder.Property(e => e.DeviceId).HasMaxLength(50);
        builder.HasIndex(e => e.SyncStatus);
    }
}
