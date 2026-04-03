using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public string PlateNumberOriginal { get; set; } = string.Empty;
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
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

public class CreateVehicleDto
{
    public string PlateNumber { get; set; } = string.Empty;
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
    public int CustomerId { get; set; }
}

public class UpdateVehicleDto : CreateVehicleDto
{
    public int Id { get; set; }
}
