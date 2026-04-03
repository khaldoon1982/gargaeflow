using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class SyncQueueEntry
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public SyncOperationType OperationType { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public SyncQueueStatus Status { get; set; } = SyncQueueStatus.Pending;
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
}
