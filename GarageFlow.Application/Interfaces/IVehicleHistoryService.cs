namespace GarageFlow.Application.Interfaces;

public interface IVehicleHistoryService
{
    Task<VehicleHistoryDto> GetHistoryAsync(int vehicleId);
    Task<CustomerHistoryDto> GetCustomerHistoryAsync(int customerId);
}

public class VehicleHistoryDto
{
    public int VehicleId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalServices { get; set; }
    public int TotalPartsReplaced { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDue { get; set; }
    public DateTime? ApkExpiryDate { get; set; }
    public List<HistoryTimelineItem> Timeline { get; set; } = new();
}

public class CustomerHistoryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int TotalVehicles { get; set; }
    public int TotalServices { get; set; }
    public DateTime? FirstVisit { get; set; }
    public DateTime? LastVisit { get; set; }
    public List<VehicleHistoryDto> Vehicles { get; set; } = new();
}

public class HistoryTimelineItem
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Cost { get; set; }
    public string? Status { get; set; }
    public List<PartLineItem> Parts { get; set; } = new();
}

public class PartLineItem
{
    public string Name { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
