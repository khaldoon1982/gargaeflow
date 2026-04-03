using System.Net.Http.Json;
using GarageFlow.Application.DTOs.Sync;
using GarageFlow.Application.Interfaces;
using Serilog;

namespace GarageFlow.Infrastructure.Sync;

public class SyncApiClient : ISyncApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public SyncApiClient(HttpClient httpClient, SyncConfiguration config, ILogger logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config.ApiUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("X-Device-Id", config.DeviceId);
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _logger = logger;
    }

    public async Task<PushResponse> PushAsync(PushRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/sync/push", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Sync push mislukt: {Status}", response.StatusCode);
                return new PushResponse();
            }
            return await response.Content.ReadFromJsonAsync<PushResponse>(ct) ?? new PushResponse();
        }
        catch (Exception ex)
        {
            _logger.Warning("Sync push fout: {Message}", ex.Message);
            return new PushResponse();
        }
    }

    public async Task<PullResponse> PullAsync(PullRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/sync/pull", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Sync pull mislukt: {Status}", response.StatusCode);
                return new PullResponse();
            }
            return await response.Content.ReadFromJsonAsync<PullResponse>(ct) ?? new PullResponse();
        }
        catch (Exception ex)
        {
            _logger.Warning("Sync pull fout: {Message}", ex.Message);
            return new PullResponse();
        }
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("health", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
