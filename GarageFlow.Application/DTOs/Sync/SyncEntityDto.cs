using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.DTOs.Sync;

public class SyncEntityDto
{
    public string EntityType { get; set; } = string.Empty;
    public Guid CloudId { get; set; }
    public string JsonPayload { get; set; } = string.Empty;
    public SyncOperationType Operation { get; set; }
    public int VersionNumber { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime LastLocalChangeAtUtc { get; set; }
}
