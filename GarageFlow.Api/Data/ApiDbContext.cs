using GarageFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    public DbSet<CloudCustomer> Customers => Set<CloudCustomer>();
    public DbSet<CloudVehicle> Vehicles => Set<CloudVehicle>();
    public DbSet<CloudMaintenanceRecord> MaintenanceRecords => Set<CloudMaintenanceRecord>();
    public DbSet<CloudInspection> Inspections => Set<CloudInspection>();
    public DbSet<CloudReminder> Reminders => Set<CloudReminder>();
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    public DbSet<SyncLog> SyncLogs => Set<SyncLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
    }
}
