using System.Text.Json;
using GarageFlow.Application.Common;
using GarageFlow.Application.DTOs.Sync;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Common;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using GarageFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GarageFlow.Infrastructure.Sync;

public class SyncService : ISyncService
{
    private static readonly string[] SyncOrder = { "Customer", "Vehicle", "MaintenanceRecord", "Inspection", "Reminder" };

    private readonly IServiceProvider _serviceProvider;
    private readonly ISyncApiClient _apiClient;
    private readonly SyncConfiguration _config;
    private readonly ILogger _logger;

    public SyncState CurrentState { get; private set; } = SyncState.Idle;
    public event EventHandler<SyncStateChangedEventArgs>? StateChanged;

    public SyncService(IServiceProvider serviceProvider, ISyncApiClient apiClient, SyncConfiguration config, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _apiClient = apiClient;
        _config = config;
        _logger = logger;
    }

    public async Task<bool> CheckConnectivityAsync(CancellationToken ct = default)
    {
        return await _apiClient.PingAsync(ct);
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        if (!_config.SyncEnabled) return;

        SetState(SyncState.Syncing);
        try
        {
            var isOnline = await _apiClient.PingAsync(ct);
            if (!isOnline)
            {
                SetState(SyncState.Offline);
                return;
            }

            await PushAsync(ct);
            await PullAsync(ct);

            SetState(SyncState.Idle);
            _logger.Information("Sync voltooid");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Sync mislukt");
            SetState(SyncState.Error, ex.Message);
        }
    }

    private async Task PushAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var queueRepo = scope.ServiceProvider.GetRequiredService<ISyncQueueRepository>();

        var pending = await queueRepo.GetPendingAsync(100);
        if (pending.Count == 0) return;

        // Group by entity type and order by sync level
        var ordered = pending
            .OrderBy(p => Array.IndexOf(SyncOrder, p.EntityName))
            .ThenBy(p => p.CreatedAtUtc)
            .ToList();

        var entities = new List<SyncEntityDto>();

        foreach (var entry in ordered)
        {
            try
            {
                var syncEntity = await BuildSyncEntityAsync(context, entry);
                if (syncEntity is not null)
                    entities.Add(syncEntity);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Kan sync entity niet opbouwen: {EntityName}#{EntityId}", entry.EntityName, entry.EntityId);
                await queueRepo.MarkFailedAsync(entry.Id, ex.Message);
            }
        }

        if (entities.Count == 0) return;

        var request = new PushRequest { DeviceId = _config.DeviceId, Entities = entities };
        var response = await _apiClient.PushAsync(request, ct);

        // Process results
        foreach (var result in response.Results)
        {
            var queueEntry = ordered.FirstOrDefault(e =>
            {
                var entity = GetSyncableByCloudId(context, e.EntityName, result.CloudId);
                return entity is not null;
            });

            if (queueEntry is not null)
            {
                if (result.IsConflict)
                {
                    await queueRepo.MarkFailedAsync(queueEntry.Id, $"Conflict: {result.ConflictDetails}");
                    await UpdateSyncStatusAsync(context, queueEntry.EntityName, result.CloudId, SyncStatus.Conflict);
                }
                else
                {
                    await queueRepo.MarkCompletedAsync(queueEntry.Id);
                    await UpdateSyncStatusAsync(context, queueEntry.EntityName, result.CloudId, SyncStatus.Synced, result.NewVersionNumber);
                }
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task PullAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();

        var lastSynced = new Dictionary<string, DateTime?>();
        foreach (var entityType in SyncOrder)
            lastSynced[entityType] = await GetLastSyncTimeAsync(context, entityType);

        var request = new PullRequest { DeviceId = _config.DeviceId, LastSyncedPerEntity = lastSynced };
        var response = await _apiClient.PullAsync(request, ct);

        if (response.Entities.Count == 0) return;

        context.IsSyncSuppressed = true;
        try
        {
            foreach (var entityType in SyncOrder)
            {
                var entitiesOfType = response.Entities.Where(e => e.EntityType == entityType).ToList();
                foreach (var syncEntity in entitiesOfType)
                {
                    await ApplyPulledEntityAsync(context, syncEntity);
                }
            }
            await context.SaveChangesAsync(ct);
        }
        finally
        {
            context.IsSyncSuppressed = false;
        }
    }

    private async Task<SyncEntityDto?> BuildSyncEntityAsync(GarageFlowDbContext context, SyncQueueEntry entry)
    {
        ISyncable? syncable = entry.EntityName switch
        {
            "Customer" => await context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == entry.EntityId),
            "Vehicle" => await context.Vehicles.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.Id == entry.EntityId),
            "MaintenanceRecord" => await context.MaintenanceRecords.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == entry.EntityId),
            "Inspection" => await context.Inspections.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == entry.EntityId),
            "Reminder" => await context.Reminders.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == entry.EntityId),
            _ => null
        };

        if (syncable is null) return null;

        return new SyncEntityDto
        {
            EntityType = entry.EntityName,
            CloudId = syncable.CloudId,
            JsonPayload = JsonSerializer.Serialize(syncable, syncable.GetType()),
            Operation = entry.OperationType,
            VersionNumber = syncable.VersionNumber,
            DeviceId = _config.DeviceId,
            LastLocalChangeAtUtc = syncable.LastLocalChangeAtUtc ?? DateTime.UtcNow
        };
    }

    private static ISyncable? GetSyncableByCloudId(GarageFlowDbContext context, string entityName, Guid cloudId)
    {
        return entityName switch
        {
            "Customer" => context.Customers.Local.FirstOrDefault(c => c.CloudId == cloudId),
            "Vehicle" => context.Vehicles.Local.FirstOrDefault(v => v.CloudId == cloudId),
            "MaintenanceRecord" => context.MaintenanceRecords.Local.FirstOrDefault(m => m.CloudId == cloudId),
            "Inspection" => context.Inspections.Local.FirstOrDefault(i => i.CloudId == cloudId),
            "Reminder" => context.Reminders.Local.FirstOrDefault(r => r.CloudId == cloudId),
            _ => null
        };
    }

    private static async Task UpdateSyncStatusAsync(GarageFlowDbContext context, string entityName, Guid cloudId, SyncStatus status, int? newVersion = null)
    {
        ISyncable? entity = entityName switch
        {
            "Customer" => await context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CloudId == cloudId),
            "Vehicle" => await context.Vehicles.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.CloudId == cloudId),
            "MaintenanceRecord" => await context.MaintenanceRecords.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.CloudId == cloudId),
            "Inspection" => await context.Inspections.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.CloudId == cloudId),
            "Reminder" => await context.Reminders.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.CloudId == cloudId),
            _ => null
        };

        if (entity is null) return;
        entity.SyncStatus = status;
        entity.LastSyncedAtUtc = DateTime.UtcNow;
        if (newVersion.HasValue)
            entity.VersionNumber = newVersion.Value;
    }

    private static async Task<DateTime?> GetLastSyncTimeAsync(GarageFlowDbContext context, string entityType)
    {
        return entityType switch
        {
            "Customer" => await context.Customers.IgnoreQueryFilters().MaxAsync(c => c.LastSyncedAtUtc),
            "Vehicle" => await context.Vehicles.IgnoreQueryFilters().MaxAsync(v => v.LastSyncedAtUtc),
            "MaintenanceRecord" => await context.MaintenanceRecords.IgnoreQueryFilters().MaxAsync(m => m.LastSyncedAtUtc),
            "Inspection" => await context.Inspections.IgnoreQueryFilters().MaxAsync(i => i.LastSyncedAtUtc),
            "Reminder" => await context.Reminders.IgnoreQueryFilters().MaxAsync(r => r.LastSyncedAtUtc),
            _ => null
        };
    }

    private Task ApplyPulledEntityAsync(GarageFlowDbContext context, SyncEntityDto syncEntity)
    {
        // Pull application will be implemented when API is ready.
        // For now, this is a placeholder that logs received entities.
        _logger.Debug("Pull entity ontvangen: {Type} {CloudId}", syncEntity.EntityType, syncEntity.CloudId);
        return Task.CompletedTask;
    }

    private void SetState(SyncState newState, string? message = null)
    {
        var old = CurrentState;
        CurrentState = newState;
        StateChanged?.Invoke(this, new SyncStateChangedEventArgs { OldState = old, NewState = newState, Message = message });
    }
}
