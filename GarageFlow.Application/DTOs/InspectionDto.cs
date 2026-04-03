using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.DTOs;

public class InspectionDto
{
    public int Id { get; set; }
    public InspectionType InspectionType { get; set; }
    public DateTime InspectionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public InspectionStatus Status { get; set; }
    public string? Notes { get; set; }
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public bool IsExpired => ExpiryDate < DateTime.Today && Status != InspectionStatus.Goedgekeurd;
    public bool IsDueSoon => ExpiryDate <= DateTime.Today.AddDays(30) && ExpiryDate >= DateTime.Today;
}

public class CreateInspectionDto
{
    public InspectionType InspectionType { get; set; }
    public DateTime InspectionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public string? Notes { get; set; }
    public int VehicleId { get; set; }
}

public class UpdateInspectionDto : CreateInspectionDto
{
    public int Id { get; set; }
    public InspectionStatus Status { get; set; }
}
