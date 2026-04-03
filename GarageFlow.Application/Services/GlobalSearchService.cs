using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;

namespace GarageFlow.Application.Services;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<Vehicle> _vehicles;
    private readonly IPlateNormalizationService _plateService;

    public GlobalSearchService(IRepository<Customer> customers, IRepository<Vehicle> vehicles, IPlateNormalizationService plateService)
    {
        _customers = customers; _vehicles = vehicles; _plateService = plateService;
    }

    public async Task<GlobalSearchResultDto> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new GlobalSearchResultDto();

        var q = query.ToLowerInvariant();
        var normalizedPlate = _plateService.Normalize(query);
        var results = new List<SearchResultItem>();

        // Search customers
        var customers = await _customers.FindAsync(c =>
            (c.FirstName != null && c.FirstName.ToLower().Contains(q)) ||
            (c.LastName != null && c.LastName.ToLower().Contains(q)) ||
            (c.CompanyName != null && c.CompanyName.ToLower().Contains(q)) ||
            c.PhoneNumber.Contains(q) ||
            (c.Email != null && c.Email.ToLower().Contains(q)) ||
            c.CustomerNumber.ToLower().Contains(q));

        foreach (var c in customers)
        {
            results.Add(new SearchResultItem
            {
                Type = "Klant",
                Id = c.Id,
                Title = c.DisplayName,
                Subtitle = $"{c.CustomerNumber} • {c.PhoneNumber}",
                Extra = $"{c.Vehicles.Count} voertuig(en)"
            });
        }

        // Search vehicles
        var vehicles = await _vehicles.FindAsync(v =>
            v.PlateNumberNormalized.Contains(normalizedPlate) ||
            v.Brand.ToLower().Contains(q) ||
            v.Model.ToLower().Contains(q) ||
            (v.ChassisNumber != null && v.ChassisNumber.ToLower().Contains(q)) ||
            (v.VIN != null && v.VIN.ToLower().Contains(q)));

        foreach (var v in vehicles)
        {
            results.Add(new SearchResultItem
            {
                Type = "Voertuig",
                Id = v.Id,
                Title = $"{v.PlateNumberOriginal} — {v.Brand} {v.Model}",
                Subtitle = v.Customer?.DisplayName ?? "",
                Extra = $"{v.Year} • {v.Mileage} km"
            });
        }

        return new GlobalSearchResultDto { Results = results.Take(20).ToList() };
    }
}
