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
        var in7Days = DateTime.Today.AddDays(7);

        var allCustomers = await _customers.GetAllAsync();
        var allVehicles = await _vehicles.GetAllAsync();
        var allMaintenance = await _maintenance.GetAllAsync();
        var allInspections = await _inspections.GetAllAsync();
        var allReminders = await _reminders.GetAllAsync();

        return new DashboardDto
        {
            TotalCustomers = allCustomers.Count(),
            TotalVehicles = allVehicles.Count(),
            MaintenanceThisMonth = allMaintenance.Count(m => m.ServiceDate >= startOfMonth && m.ServiceDate <= now),
            InspectionsDueIn7Days = allInspections.Count(i => i.ExpiryDate <= in7Days && i.ExpiryDate >= DateTime.Today),
            ExpiredInspections = allInspections.Count(i => i.ExpiryDate < DateTime.Today && i.Status != InspectionStatus.Goedgekeurd),
            OpenReminders = allReminders.Count(r => r.Status == ReminderStatus.Openstaand),
            RecentMaintenance = allMaintenance.OrderByDescending(m => m.ServiceDate).Take(5).Select(m => new MaintenanceRecordDto
            {
                Id = m.Id, ServiceDate = m.ServiceDate, ServiceType = m.ServiceType, Description = m.Description,
                TotalCost = m.TotalCost, Status = m.Status, VehiclePlate = m.Vehicle?.PlateNumberOriginal ?? "",
                CustomerName = m.Vehicle?.Customer?.DisplayName ?? ""
            }).ToList(),
            UpcomingInspections = allInspections.Where(i => i.ExpiryDate >= DateTime.Today).OrderBy(i => i.ExpiryDate).Take(5).Select(i => new InspectionDto
            {
                Id = i.Id, InspectionType = i.InspectionType, ExpiryDate = i.ExpiryDate, Status = i.Status,
                VehiclePlate = i.Vehicle?.PlateNumberOriginal ?? "", CustomerName = i.Vehicle?.Customer?.DisplayName ?? ""
            }).ToList()
        };
    }
}
