using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class MaintenanceRecord : BaseEntity, ISoftDeletable, ISyncable
{
    public int Id { get; set; }
    public DateTime ServiceDate { get; set; }
    public int MileageAtService { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PartsChanged { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? TechnicianName { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public int? NextServiceMileage { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Gepland;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Sync
    public Guid CloudId { get; set; } = Guid.NewGuid();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingUpload;
    public DateTime? LastSyncedAtUtc { get; set; }
    public DateTime? LastLocalChangeAtUtc { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string? DeviceId { get; set; }

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
