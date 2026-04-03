namespace GarageFlow.Application.DTOs.Sync;

public class PullResponse
{
    public List<SyncEntityDto> Entities { get; set; } = new();
    public DateTime ServerTimestampUtc { get; set; }
}
