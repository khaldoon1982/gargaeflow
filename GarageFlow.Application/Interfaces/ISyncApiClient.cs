using GarageFlow.Application.DTOs.Sync;

namespace GarageFlow.Application.Interfaces;

public interface ISyncApiClient
{
    Task<PushResponse> PushAsync(PushRequest request, CancellationToken ct = default);
    Task<PullResponse> PullAsync(PullRequest request, CancellationToken ct = default);
    Task<bool> PingAsync(CancellationToken ct = default);
}
