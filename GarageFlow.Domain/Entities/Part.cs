using GarageFlow.Domain.Common;

namespace GarageFlow.Domain.Entities;

public class Part : BaseEntity, ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public int MaintenanceRecordId { get; set; }
    public MaintenanceRecord MaintenanceRecord { get; set; } = null!;
}
