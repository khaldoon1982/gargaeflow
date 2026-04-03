using GarageFlow.Domain.Common;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Persistence.Context;

public class GarageFlowDbContext : DbContext
{
    private static readonly HashSet<string> SyncMetadataProperties = new()
    {
        nameof(ISyncable.SyncStatus),
        nameof(ISyncable.LastSyncedAtUtc),
        nameof(ISyncable.LastLocalChangeAtUtc),
        nameof(ISyncable.VersionNumber),
        nameof(ISyncable.DeviceId)
    };

    public GarageFlowDbContext(DbContextOptions<GarageFlowDbContext> options) : base(options) { }

    /// <summary>
    /// Set to true during pull/apply from server to prevent re-enqueuing synced data.
    /// </summary>
    public bool IsSyncSuppressed { get; set; }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<SyncQueueEntry> SyncQueue => Set<SyncQueueEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GarageFlowDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }

        if (!IsSyncSuppressed)
        {
            // Collect entries first to avoid modifying ChangeTracker during iteration
            var syncEntries = new List<(ISyncable Entity, SyncOperationType Op)>();

            foreach (var entry in ChangeTracker.Entries<ISyncable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.LastLocalChangeAtUtc = now;
                    entry.Entity.SyncStatus = SyncStatus.PendingUpload;
                    syncEntries.Add((entry.Entity, SyncOperationType.Create));
                }
                else if (entry.State == EntityState.Modified)
                {
                    var changedProps = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name)
                        .ToHashSet();

                    if (changedProps.IsSubsetOf(SyncMetadataProperties))
                        continue;

                    entry.Entity.LastLocalChangeAtUtc = now;
                    entry.Entity.SyncStatus = SyncStatus.PendingUpload;

                    var opType = SyncOperationType.Update;
                    if (entry.Entity is ISoftDeletable sd)
                    {
                        var isActiveProp = entry.Property(nameof(ISoftDeletable.IsActive));
                        if (isActiveProp.IsModified && !sd.IsActive)
                            opType = SyncOperationType.Delete;
                    }

                    syncEntries.Add((entry.Entity, opType));
                }
            }

            // Now enqueue after iteration is complete
            foreach (var (entity, op) in syncEntries)
                EnqueueSync(entity, op);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnqueueSync(ISyncable entity, SyncOperationType operationType)
    {
        var entityType = entity.GetType().Name;
        var idProp = entity.GetType().GetProperty("Id");
        var entityId = (int)(idProp?.GetValue(entity) ?? 0);

        SyncQueue.Add(new SyncQueueEntry
        {
            EntityName = entityType,
            EntityId = entityId,
            OperationType = operationType,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
