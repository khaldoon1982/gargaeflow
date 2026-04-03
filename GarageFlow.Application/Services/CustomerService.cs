using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using Serilog;

namespace GarageFlow.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repo;
    private readonly ISettingsService _settings;
    private readonly ILogger _logger;

    public CustomerService(IRepository<Customer> repo, ISettingsService settings, ILogger logger) { _repo = repo; _settings = settings; _logger = logger; }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c is null ? null : MapToDto(c);
    }

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string query)
    {
        var q = query.ToLowerInvariant();
        var items = await _repo.FindAsync(c =>
            (c.FirstName != null && c.FirstName.ToLower().Contains(q)) ||
            (c.LastName != null && c.LastName.ToLower().Contains(q)) ||
            (c.CompanyName != null && c.CompanyName.ToLower().Contains(q)) ||
            (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(q)) ||
            c.PhoneNumber.Contains(q) ||
            (c.Email != null && c.Email.ToLower().Contains(q)) ||
            c.CustomerNumber.ToLower().Contains(q));
        return items.Select(MapToDto);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customerNumber = await GenerateCustomerNumberAsync();
        var entity = new Customer
        {
            CustomerNumber = customerNumber,
            CustomerType = dto.CustomerType,
            FirstName = dto.FirstName, LastName = dto.LastName,
            CompanyName = dto.CompanyName, ContactPerson = dto.ContactPerson, Department = dto.Department,
            PhoneNumber = dto.PhoneNumber, WhatsAppNumber = dto.WhatsAppNumber, Email = dto.Email,
            BillingEmail = dto.BillingEmail, PreferredContactMethod = dto.PreferredContactMethod,
            Street = dto.Street, HouseNumber = dto.HouseNumber, HouseNumberAddition = dto.HouseNumberAddition,
            PostalCode = dto.PostalCode, City = dto.City, Country = dto.Country,
            ChamberOfCommerceNumber = dto.ChamberOfCommerceNumber, VATNumber = dto.VATNumber,
            PaymentTerm = dto.PaymentTerm, DebtorNumber = dto.DebtorNumber, FleetManager = dto.FleetManager,
            BillingNotes = dto.BillingNotes, Notes = dto.Notes,
            LastActivityAt = DateTime.UtcNow
        };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        _logger.Information("Klant aangemaakt: {Id} {Number}", created.Id, created.CustomerNumber);
        return MapToDto(created);
    }

    public async Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto)
    {
        var c = await _repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Klant met Id {dto.Id} niet gevonden.");
        c.CustomerType = dto.CustomerType;
        c.FirstName = dto.FirstName; c.LastName = dto.LastName;
        c.CompanyName = dto.CompanyName; c.ContactPerson = dto.ContactPerson; c.Department = dto.Department;
        c.PhoneNumber = dto.PhoneNumber; c.WhatsAppNumber = dto.WhatsAppNumber; c.Email = dto.Email;
        c.BillingEmail = dto.BillingEmail; c.PreferredContactMethod = dto.PreferredContactMethod;
        c.Street = dto.Street; c.HouseNumber = dto.HouseNumber; c.HouseNumberAddition = dto.HouseNumberAddition;
        c.PostalCode = dto.PostalCode; c.City = dto.City; c.Country = dto.Country;
        c.ChamberOfCommerceNumber = dto.ChamberOfCommerceNumber; c.VATNumber = dto.VATNumber;
        c.PaymentTerm = dto.PaymentTerm; c.DebtorNumber = dto.DebtorNumber; c.FleetManager = dto.FleetManager;
        c.BillingNotes = dto.BillingNotes; c.Notes = dto.Notes;
        c.IsArchived = dto.IsArchived;
        c.LastActivityAt = DateTime.UtcNow;
        await _repo.UpdateAsync(c);
        await _repo.SaveChangesAsync();
        return MapToDto(c);
    }

    public async Task DeleteAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Klant met Id {id} niet gevonden.");
        if (c.Vehicles.Any())
            throw new InvalidOperationException("Deze klant kan niet worden verwijderd omdat er gekoppelde gegevens bestaan. Archiveer de klant in plaats daarvan.");
        c.IsActive = false;
        await _repo.UpdateAsync(c);
        await _repo.SaveChangesAsync();
    }

    private async Task<string> GenerateCustomerNumberAsync()
    {
        var nextStr = await _settings.GetAsync("NextCustomerNumber") ?? "1";
        var next = int.Parse(nextStr);
        await _settings.SetAsync("NextCustomerNumber", (next + 1).ToString());
        return $"KL-{next:D6}";
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id, CustomerNumber = c.CustomerNumber, CustomerType = c.CustomerType,
        DisplayName = c.DisplayName, FirstName = c.FirstName, LastName = c.LastName,
        CompanyName = c.CompanyName, ContactPerson = c.ContactPerson, Department = c.Department,
        PhoneNumber = c.PhoneNumber, WhatsAppNumber = c.WhatsAppNumber, Email = c.Email,
        BillingEmail = c.BillingEmail, PreferredContactMethod = c.PreferredContactMethod,
        Street = c.Street, HouseNumber = c.HouseNumber, HouseNumberAddition = c.HouseNumberAddition,
        PostalCode = c.PostalCode, City = c.City, Country = c.Country,
        ChamberOfCommerceNumber = c.ChamberOfCommerceNumber, VATNumber = c.VATNumber,
        PaymentTerm = c.PaymentTerm, DebtorNumber = c.DebtorNumber, FleetManager = c.FleetManager,
        BillingNotes = c.BillingNotes, Notes = c.Notes, IsArchived = c.IsArchived,
        LastActivityAt = c.LastActivityAt, VehicleCount = c.Vehicles.Count
    };
}
