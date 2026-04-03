using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<Vehicle> _vehicles;
    private readonly IRepository<MaintenanceRecord> _maintenance;
    private readonly IRepository<Inspection> _inspections;
    private readonly IRepository<Reminder> _reminders;

    public DashboardService(IRepository<Customer> customers, IRepository<Vehicle> vehicles, IRepository<MaintenanceRecord> maintenance, IRepository<Inspection> inspections, IRepository<Reminder> reminders)
    {
        _customers = customers; _vehicles = vehicles; _maintenance = maintenance; _inspections = inspections; _reminders = reminders;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var in7Days = DateTime.Today.AddDays(7);

        var allCustomers = await _customers.GetAllAsync();
        var allVehicles = await _vehicles.GetAllAsync();
        var allMaintenance = await _maintenance.GetAllAsync();
        var allInspections = await _inspections.GetAllAsync();
        var allReminders = await _reminders.GetAllAsync();

        var maintenanceThisMonth = allMaintenance.Where(m => m.ServiceDate >= startOfMonth && m.ServiceDate <= now).ToList();
        var maintenanceLastMonth = allMaintenance.Where(m => m.ServiceDate >= startOfLastMonth && m.ServiceDate < startOfMonth).ToList();
        var completedJobs = allMaintenance.Where(m => m.Status == MaintenanceStatus.Afgerond).ToList();

        var alerts = new List<VehicleAlertDto>();
        foreach (var v in allVehicles)
        {
            if (v.InspectionExpiryDate.HasValue && v.InspectionExpiryDate.Value < DateTime.Today)
                alerts.Add(new VehicleAlertDto { VehicleId = v.Id, PlateNumber = v.PlateNumberOriginal, CustomerName = v.Customer?.DisplayName ?? "", AlertType = "APK Verlopen", Message = $"APK verlopen op {v.InspectionExpiryDate.Value:dd-MM-yyyy}", DueDate = v.InspectionExpiryDate });
            else if (v.InspectionExpiryDate.HasValue && v.InspectionExpiryDate.Value <= in7Days)
                alerts.Add(new VehicleAlertDto { VehicleId = v.Id, PlateNumber = v.PlateNumberOriginal, CustomerName = v.Customer?.DisplayName ?? "", AlertType = "APK Binnenkort", Message = $"APK verloopt op {v.InspectionExpiryDate.Value:dd-MM-yyyy}", DueDate = v.InspectionExpiryDate });
        }

        return new DashboardDto
        {
            TotalCustomers = allCustomers.Count(),
            TotalVehicles = allVehicles.Count(),
            MaintenanceThisMonth = maintenanceThisMonth.Count,
            InspectionsDueIn7Days = allInspections.Count(i => i.ExpiryDate <= in7Days && i.ExpiryDate >= DateTime.Today),
            ExpiredInspections = allInspections.Count(i => i.ExpiryDate < DateTime.Today && i.Status != InspectionStatus.Goedgekeurd),
            OpenReminders = allReminders.Count(r => r.Status == ReminderStatus.Openstaand),
            ActiveMaintenanceJobs = allMaintenance.Count(m => m.Status == MaintenanceStatus.InBehandeling || m.Status == MaintenanceStatus.Gepland),
            RevenueThisMonth = maintenanceThisMonth.Sum(m => m.TotalCost),
            RevenueLastMonth = maintenanceLastMonth.Sum(m => m.TotalCost),
            TotalOutstandingCosts = allMaintenance.Where(m => m.Status == MaintenanceStatus.Gepland || m.Status == MaintenanceStatus.InBehandeling).Sum(m => m.TotalCost),
            AverageJobCost = completedJobs.Count > 0 ? completedJobs.Average(m => m.TotalCost) : 0,
            RecentMaintenance = allMaintenance.OrderByDescending(m => m.ServiceDate).Take(8).Select(m => new MaintenanceRecordDto
            {
                Id = m.Id, ServiceDate = m.ServiceDate, ServiceType = m.ServiceType, Description = m.Description,
                TotalCost = m.TotalCost, Status = m.Status, TechnicianName = m.TechnicianName,
                VehiclePlate = m.Vehicle?.PlateNumberOriginal ?? "", CustomerName = m.Vehicle?.Customer?.DisplayName ?? ""
            }).ToList(),
            UpcomingInspections = allInspections.Where(i => i.ExpiryDate >= DateTime.Today).OrderBy(i => i.ExpiryDate).Take(5).Select(i => new InspectionDto
            {
                Id = i.Id, InspectionType = i.InspectionType, InspectionDate = i.InspectionDate, ExpiryDate = i.ExpiryDate, Status = i.Status,
                VehiclePlate = i.Vehicle?.PlateNumberOriginal ?? "", CustomerName = i.Vehicle?.Customer?.DisplayName ?? ""
            }).ToList(),
            ActiveJobs = allMaintenance.Where(m => m.Status == MaintenanceStatus.InBehandeling || m.Status == MaintenanceStatus.Gepland).OrderBy(m => m.ServiceDate).Take(10).Select(m => new MaintenanceRecordDto
            {
                Id = m.Id, ServiceDate = m.ServiceDate, ServiceType = m.ServiceType, Description = m.Description,
                TotalCost = m.TotalCost, Status = m.Status, TechnicianName = m.TechnicianName,
                VehiclePlate = m.Vehicle?.PlateNumberOriginal ?? "", CustomerName = m.Vehicle?.Customer?.DisplayName ?? ""
            }).ToList(),
            VehicleAlerts = alerts.OrderBy(a => a.DueDate).Take(10).ToList()
        };
    }
}
