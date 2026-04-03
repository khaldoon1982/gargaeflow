using GarageFlow.Application.Common;

namespace GarageFlow.Application.Interfaces;

public interface ISyncService
{
    Task SyncAsync(CancellationToken ct = default);
    Task<bool> CheckConnectivityAsync(CancellationToken ct = default);
    SyncState CurrentState { get; }
    event EventHandler<SyncStateChangedEventArgs>? StateChanged;
}
