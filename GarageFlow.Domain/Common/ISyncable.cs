using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Common;

public interface ISyncable
{
    Guid CloudId { get; set; }
    SyncStatus SyncStatus { get; set; }
    DateTime? LastSyncedAtUtc { get; set; }
    DateTime? LastLocalChangeAtUtc { get; set; }
    int VersionNumber { get; set; }
    string? DeviceId { get; set; }
}
