using GarageFlow.Api.Data;
using GarageFlow.Api.Models;
using GarageFlow.Application.DTOs.Sync;
using GarageFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Api.Services;

public class SyncProcessor
{
    private static readonly Dictionary<string, int> SyncLevels = new()
    {
        ["Customer"] = 0, ["Vehicle"] = 1,
        ["MaintenanceRecord"] = 2, ["Inspection"] = 2, ["Reminder"] = 2
    };

    private readonly ApiDbContext _db;

    public SyncProcessor(ApiDbContext db) { _db = db; }

    public async Task<PushResponse> ProcessPushAsync(PushRequest request)
    {
        var results = new List<SyncEntityResult>();
        var ordered = request.Entities.OrderBy(e => SyncLevels.GetValueOrDefault(e.EntityType, 99)).ToList();

        foreach (var entity in ordered)
        {
            var result = await ProcessSinglePushAsync(entity, request.DeviceId);
            results.Add(result);
        }

        await _db.SyncLogs.AddAsync(new SyncLog
        {
            DeviceId = request.DeviceId,
            Direction = "push",
            EntityCount = results.Count(r => !r.IsConflict)
        });
        await _db.SaveChangesAsync();

        return new PushResponse { Results = results, ServerTimestampUtc = DateTime.UtcNow };
    }

    public async Task<PullResponse> ProcessPullAsync(PullRequest request)
    {
        var entities = new List<SyncEntityDto>();

        foreach (var (entityType, lastSynced) in request.LastSyncedPerEntity)
        {
            var since = lastSynced ?? DateTime.MinValue;
            var changed = entityType switch
            {
                "Customer" => await GetChangedEntities<CloudCustomer>(since, request.DeviceId),
                "Vehicle" => await GetChangedEntities<CloudVehicle>(since, request.DeviceId),
                "MaintenanceRecord" => await GetChangedEntities<CloudMaintenanceRecord>(since, request.DeviceId),
                "Inspection" => await GetChangedEntities<CloudInspection>(since, request.DeviceId),
                "Reminder" => await GetChangedEntities<CloudReminder>(since, request.DeviceId),
                _ => new List<SyncEntityDto>()
            };
            entities.AddRange(changed);
        }

        await _db.SyncLogs.AddAsync(new SyncLog
        {
            DeviceId = request.DeviceId,
            Direction = "pull",
            EntityCount = entities.Count
        });
        await _db.SaveChangesAsync();

        return new PullResponse { Entities = entities, ServerTimestampUtc = DateTime.UtcNow };
    }

    private async Task<SyncEntityResult> ProcessSinglePushAsync(SyncEntityDto dto, string deviceId)
    {
        return dto.EntityType switch
        {
            "Customer" => await UpsertEntity<CloudCustomer>(dto, deviceId),
            "Vehicle" => await UpsertEntity<CloudVehicle>(dto, deviceId),
            "MaintenanceRecord" => await UpsertEntity<CloudMaintenanceRecord>(dto, deviceId),
            "Inspection" => await UpsertEntity<CloudInspection>(dto, deviceId),
            "Reminder" => await UpsertEntity<CloudReminder>(dto, deviceId),
            _ => new SyncEntityResult { EntityType = dto.EntityType, CloudId = dto.CloudId, IsConflict = true, ConflictDetails = "Unknown entity type" }
        };
    }

    private async Task<SyncEntityResult> UpsertEntity<T>(SyncEntityDto dto, string deviceId) where T : CloudEntityBase, new()
    {
        var existing = await _db.Set<T>().FirstOrDefaultAsync(e => e.CloudId == dto.CloudId);

        if (existing is null)
        {
            var entity = new T
            {
                CloudId = dto.CloudId,
                VersionNumber = 1,
                DeviceId = deviceId,
                JsonData = dto.JsonPayload,
                IsDeleted = dto.Operation == SyncOperationType.Delete,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _db.Set<T>().Add(entity);
            await _db.SaveChangesAsync();

            return new SyncEntityResult { EntityType = dto.EntityType, CloudId = dto.CloudId, NewVersionNumber = 1 };
        }

        // Optimistic concurrency check
        if (existing.VersionNumber != dto.VersionNumber)
        {
            return new SyncEntityResult
            {
                EntityType = dto.EntityType,
                CloudId = dto.CloudId,
                NewVersionNumber = existing.VersionNumber,
                IsConflict = true,
                ConflictDetails = $"Versie conflict: client={dto.VersionNumber}, server={existing.VersionNumber}"
            };
        }

        existing.VersionNumber++;
        existing.DeviceId = deviceId;
        existing.JsonData = dto.JsonPayload;
        existing.IsDeleted = dto.Operation == SyncOperationType.Delete;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new SyncEntityResult { EntityType = dto.EntityType, CloudId = dto.CloudId, NewVersionNumber = existing.VersionNumber };
    }

    private async Task<List<SyncEntityDto>> GetChangedEntities<T>(DateTime since, string excludeDeviceId) where T : CloudEntityBase
    {
        var entities = await _db.Set<T>()
            .Where(e => e.UpdatedAtUtc > since && e.DeviceId != excludeDeviceId)
            .ToListAsync();

        var typeName = typeof(T).Name.Replace("Cloud", "");

        return entities.Select(e => new SyncEntityDto
        {
            EntityType = typeName,
            CloudId = e.CloudId,
            JsonPayload = e.JsonData,
            Operation = e.IsDeleted ? SyncOperationType.Delete : SyncOperationType.Update,
            VersionNumber = e.VersionNumber,
            DeviceId = e.DeviceId,
            LastLocalChangeAtUtc = e.UpdatedAtUtc
        }).ToList();
    }
}
