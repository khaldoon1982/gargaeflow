namespace GarageFlow.Application.DTOs;

public class DashboardDto
{
    // Counts
    public int TotalCustomers { get; set; }
    public int TotalVehicles { get; set; }
    public int MaintenanceThisMonth { get; set; }
    public int InspectionsDueIn7Days { get; set; }
    public int ExpiredInspections { get; set; }
    public int OpenReminders { get; set; }
    public int ActiveMaintenanceJobs { get; set; }

    // Financials
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal TotalOutstandingCosts { get; set; }
    public decimal AverageJobCost { get; set; }

    // Lists
    public List<MaintenanceRecordDto> RecentMaintenance { get; set; } = new();
    public List<InspectionDto> UpcomingInspections { get; set; } = new();
    public List<MaintenanceRecordDto> ActiveJobs { get; set; } = new();
    public List<VehicleAlertDto> VehicleAlerts { get; set; } = new();
}

public class VehicleAlertDto
{
    public int VehicleId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}
