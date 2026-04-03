namespace GarageFlow.Domain.Enums;

public enum SyncStatus
{
    Synced = 0,
    PendingUpload = 1,
    PendingDownload = 2,
    Conflict = 3
}
