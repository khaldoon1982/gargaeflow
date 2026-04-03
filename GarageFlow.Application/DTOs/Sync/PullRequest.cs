namespace GarageFlow.Application.DTOs.Sync;

public class PullRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public Dictionary<string, DateTime?> LastSyncedPerEntity { get; set; } = new();
}
