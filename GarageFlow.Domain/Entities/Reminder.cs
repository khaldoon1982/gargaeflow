using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class Reminder : BaseEntity, ISoftDeletable, ISyncable
{
    public int Id { get; set; }
    public ReminderType ReminderType { get; set; }
    public DateTime ReminderDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public SendMethod SendMethod { get; set; } = SendMethod.Intern;
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Openstaand;
    public bool IsActive { get; set; } = true;

    // Sync
    public Guid CloudId { get; set; } = Guid.NewGuid();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingUpload;
    public DateTime? LastSyncedAtUtc { get; set; }
    public DateTime? LastLocalChangeAtUtc { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string? DeviceId { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}
