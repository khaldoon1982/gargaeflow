using GarageFlow.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GarageFlow.Infrastructure.Sync;

public class BackgroundSyncService : IHostedService, IDisposable
{
    private readonly ISyncService _syncService;
    private readonly SyncConfiguration _config;
    private readonly ILogger _logger;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public BackgroundSyncService(ISyncService syncService, SyncConfiguration config, ILogger logger)
    {
        _syncService = syncService;
        _config = config;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_config.SyncEnabled)
        {
            _logger.Information("Sync is uitgeschakeld (GARAGEFLOW_SYNC_ENABLED=false)");
            return Task.CompletedTask;
        }

        _logger.Information("Background sync gestart (interval: {Seconds}s, device: {DeviceId})",
            _config.SyncIntervalSeconds, _config.DeviceId);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(_config.SyncIntervalSeconds));
        _executingTask = RunAsync(_cts.Token);

        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        // Initial delay to let the app fully start
        await Task.Delay(TimeSpan.FromSeconds(5), ct);

        while (await _timer!.WaitForNextTickAsync(ct))
        {
            try
            {
                await _syncService.SyncAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Background sync cyclus mislukt");
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        if (_executingTask is not null)
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        _logger.Information("Background sync gestopt");
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _timer?.Dispose();
        _cts?.Dispose();
    }
}
