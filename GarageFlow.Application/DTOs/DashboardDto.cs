namespace GarageFlow.Application.DTOs;

public class DashboardDto
{
    public int TotalCustomers { get; set; }
    public int TotalVehicles { get; set; }
    public int MaintenanceThisMonth { get; set; }
    public int InspectionsDueIn7Days { get; set; }
    public int ExpiredInspections { get; set; }
    public int OpenReminders { get; set; }
    public List<MaintenanceRecordDto> RecentMaintenance { get; set; } = new();
    public List<InspectionDto> UpcomingInspections { get; set; } = new();
}
