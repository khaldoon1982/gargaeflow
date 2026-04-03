using GarageFlow.Application.DTOs;
using GarageFlow.Application.Services;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using GarageFlow.Persistence.Context;
using GarageFlow.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Xunit;

namespace GarageFlow.Tests.Services;

public class CustomerServiceTests : IDisposable
{
    private readonly GarageFlowDbContext _context;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<GarageFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new GarageFlowDbContext(options);
        var repository = new Repository<Customer>(_context);
        var settingsService = new SettingsService(_context);
        _service = new CustomerService(repository, settingsService, new LoggerConfiguration().CreateLogger());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePrivateCustomer()
    {
        var result = await _service.CreateAsync(new CreateCustomerDto
        {
            CustomerType = CustomerType.Private,
            FirstName = "Test",
            LastName = "Klant",
            PhoneNumber = "06-12345678",
            Email = "test@test.nl"
        });

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Klant", result.DisplayName);
        Assert.StartsWith("KL-", result.CustomerNumber);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBusinessCustomer()
    {
        var result = await _service.CreateAsync(new CreateCustomerDto
        {
            CustomerType = CustomerType.Business,
            CompanyName = "Test B.V.",
            ContactPerson = "Jan",
            PhoneNumber = "020-1234567"
        });

        Assert.Equal("Test B.V.", result.DisplayName);
        Assert.Equal(CustomerType.Business, result.CustomerType);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveCustomers()
    {
        _context.Customers.AddRange(
            new Customer { CustomerNumber = "KL-001", FirstName = "A", LastName = "B", PhoneNumber = "06-1" },
            new Customer { CustomerNumber = "KL-002", FirstName = "C", LastName = "D", PhoneNumber = "06-2", IsActive = false }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByName()
    {
        _context.Customers.Add(new Customer { CustomerNumber = "KL-001", FirstName = "Mohammed", LastName = "El Amrani", PhoneNumber = "06-1" });
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync("mohammed");
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByCompanyName()
    {
        _context.Customers.Add(new Customer { CustomerNumber = "KL-002", CustomerType = CustomerType.Business, CompanyName = "ABC Logistics", PhoneNumber = "020-1" });
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync("logistics");
        Assert.Single(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenHasVehicles()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        _context.Vehicles.Add(new Vehicle { PlateNumberOriginal = "AA-11-BB", PlateNumberNormalized = "AA11BB", Brand = "VW", Model = "Golf", Year = 2020, CustomerId = customer.Id });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(customer.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteWhenNoVehicles()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "Del", LastName = "Test", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(customer.Id);
        var deleted = await _context.Customers.IgnoreQueryFilters().FirstAsync(c => c.Id == customer.Id);
        Assert.False(deleted.IsActive);
    }

    public void Dispose() { _context.Dispose(); }
}
