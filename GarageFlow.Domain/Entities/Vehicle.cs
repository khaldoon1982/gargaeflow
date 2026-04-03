using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class Vehicle : BaseEntity, ISoftDeletable, ISyncable
{
    public int Id { get; set; }
    public string PlateNumberOriginal { get; set; } = string.Empty;
    public string PlateNumberNormalized { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Trim { get; set; }
    public int Year { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? VIN { get; set; }
    public FuelType FuelType { get; set; }
    public TransmissionType TransmissionType { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public DateTime? InspectionDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public bool IsActive { get; set; } = true;

    // Sync
    public Guid CloudId { get; set; } = Guid.NewGuid();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingUpload;
    public DateTime? LastSyncedAtUtc { get; set; }
    public DateTime? LastLocalChangeAtUtc { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string? DeviceId { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
