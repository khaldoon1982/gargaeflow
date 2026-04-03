using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.DTOs;

public class MaintenanceRecordDto
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
    public MaintenanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}

public class CreateMaintenanceRecordDto
{
    public DateTime ServiceDate { get; set; }
    public int MileageAtService { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PartsChanged { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public string? TechnicianName { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public int? NextServiceMileage { get; set; }
    public string? Notes { get; set; }
    public int VehicleId { get; set; }
}

public class UpdateMaintenanceRecordDto : CreateMaintenanceRecordDto
{
    public int Id { get; set; }
    public MaintenanceStatus Status { get; set; }
}
