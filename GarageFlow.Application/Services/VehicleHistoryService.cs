using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;

namespace GarageFlow.Application.Services;

public class VehicleHistoryService : IVehicleHistoryService
{
    private readonly IRepository<Vehicle> _vehicles;
    private readonly IRepository<MaintenanceRecord> _maintenance;
    private readonly IRepository<Inspection> _inspections;
    private readonly IRepository<Part> _parts;
    private readonly IRepository<Customer> _customers;

    public VehicleHistoryService(IRepository<Vehicle> vehicles, IRepository<MaintenanceRecord> maintenance, IRepository<Inspection> inspections, IRepository<Part> parts, IRepository<Customer> customers)
    {
        _vehicles = vehicles; _maintenance = maintenance; _inspections = inspections; _parts = parts; _customers = customers;
    }

    public async Task<VehicleHistoryDto> GetHistoryAsync(int vehicleId)
    {
        var vehicle = await _vehicles.GetByIdAsync(vehicleId);
        if (vehicle is null) return new VehicleHistoryDto();

        var maintenance = (await _maintenance.FindAsync(m => m.VehicleId == vehicleId)).ToList();
        var inspections = (await _inspections.FindAsync(i => i.VehicleId == vehicleId)).ToList();
        var allParts = new List<Part>();
        foreach (var m in maintenance)
        {
            var parts = await _parts.FindAsync(p => p.MaintenanceRecordId == m.Id);
            allParts.AddRange(parts);
        }

        var timeline = new List<HistoryTimelineItem>();

        // Maintenance entries
        foreach (var m in maintenance)
        {
            var mParts = allParts.Where(p => p.MaintenanceRecordId == m.Id).ToList();
            timeline.Add(new HistoryTimelineItem
            {
                Date = m.ServiceDate,
                Type = "Onderhoud",
                Title = m.ServiceType,
                Description = m.Description,
                Cost = m.TotalCost,
                Status = m.Status.ToString(),
                Parts = mParts.Select(p => new PartLineItem
                {
                    Name = p.Name, PartNumber = p.PartNumber, Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice, TotalPrice = p.TotalPrice
                }).ToList()
            });
        }

        // Inspections
        foreach (var i in inspections)
        {
            timeline.Add(new HistoryTimelineItem
            {
                Date = i.InspectionDate,
                Type = "Keuring",
                Title = i.InspectionType.ToString(),
                Description = $"Vervaldatum: {i.ExpiryDate:dd-MM-yyyy}",
                Status = i.Status.ToString()
            });
        }

        return new VehicleHistoryDto
        {
            VehicleId = vehicle.Id,
            PlateNumber = vehicle.PlateNumberOriginal,
            VehicleName = $"{vehicle.Brand} {vehicle.Model} ({vehicle.Year})",
            CustomerName = vehicle.Customer?.DisplayName ?? "",
            CustomerId = vehicle.CustomerId,
            TotalSpent = maintenance.Sum(m => m.TotalCost),
            TotalServices = maintenance.Count,
            TotalPartsReplaced = allParts.Sum(p => p.Quantity),
            LastServiceDate = maintenance.OrderByDescending(m => m.ServiceDate).FirstOrDefault()?.ServiceDate,
            NextServiceDue = vehicle.NextServiceDate,
            ApkExpiryDate = vehicle.InspectionExpiryDate,
            Timeline = timeline.OrderByDescending(t => t.Date).ToList()
        };
    }

    public async Task<CustomerHistoryDto> GetCustomerHistoryAsync(int customerId)
    {
        var customer = await _customers.GetByIdAsync(customerId);
        if (customer is null) return new CustomerHistoryDto();

        var vehicles = (await _vehicles.FindAsync(v => v.CustomerId == customerId)).ToList();
        var vehicleHistories = new List<VehicleHistoryDto>();
        decimal totalSpent = 0;
        int totalServices = 0;
        DateTime? firstVisit = null;
        DateTime? lastVisit = null;

        foreach (var v in vehicles)
        {
            var history = await GetHistoryAsync(v.Id);
            vehicleHistories.Add(history);
            totalSpent += history.TotalSpent;
            totalServices += history.TotalServices;

            var earliest = history.Timeline.MinBy(t => t.Date)?.Date;
            var latest = history.Timeline.MaxBy(t => t.Date)?.Date;
            if (earliest.HasValue && (firstVisit is null || earliest < firstVisit)) firstVisit = earliest;
            if (latest.HasValue && (lastVisit is null || latest > lastVisit)) lastVisit = latest;
        }

        return new CustomerHistoryDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.DisplayName,
            TotalSpent = totalSpent,
            TotalVehicles = vehicles.Count,
            TotalServices = totalServices,
            FirstVisit = firstVisit,
            LastVisit = lastVisit,
            Vehicles = vehicleHistories
        };
    }
}
