using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class Inspection : BaseEntity, ISoftDeletable, ISyncable
{
    public int Id { get; set; }
    public InspectionType InspectionType { get; set; }
    public DateTime InspectionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public InspectionStatus Status { get; set; } = InspectionStatus.Gepland;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Sync
    public Guid CloudId { get; set; } = Guid.NewGuid();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingUpload;
    public DateTime? LastSyncedAtUtc { get; set; }
    public DateTime? LastLocalChangeAtUtc { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string? DeviceId { get; set; }

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
}
