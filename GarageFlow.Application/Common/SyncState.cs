namespace GarageFlow.Application.Common;

public enum SyncState
{
    Idle,
    Syncing,
    Error,
    Offline
}

public class SyncStateChangedEventArgs : EventArgs
{
    public SyncState OldState { get; init; }
    public SyncState NewState { get; init; }
    public string? Message { get; init; }
}
