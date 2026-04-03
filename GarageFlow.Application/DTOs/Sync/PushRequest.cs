namespace GarageFlow.Application.DTOs.Sync;

public class PushRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public List<SyncEntityDto> Entities { get; set; } = new();
}
