namespace GarageFlow.Application.DTOs.Sync;

public class PushResponse
{
    public List<SyncEntityResult> Results { get; set; } = new();
    public DateTime ServerTimestampUtc { get; set; }
}

public class SyncEntityResult
{
    public string EntityType { get; set; } = string.Empty;
    public Guid CloudId { get; set; }
    public int NewVersionNumber { get; set; }
    public bool IsConflict { get; set; }
    public string? ConflictDetails { get; set; }
}
