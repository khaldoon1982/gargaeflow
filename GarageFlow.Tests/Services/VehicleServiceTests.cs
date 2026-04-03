using GarageFlow.Application.DTOs;
using GarageFlow.Application.Services;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using GarageFlow.Infrastructure.Services;
using GarageFlow.Persistence.Context;
using GarageFlow.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Xunit;

namespace GarageFlow.Tests.Services;

public class VehicleServiceTests : IDisposable
{
    private readonly GarageFlowDbContext _context;
    private readonly VehicleService _service;

    public VehicleServiceTests()
    {
        var options = new DbContextOptionsBuilder<GarageFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new GarageFlowDbContext(options);
        var repository = new Repository<Vehicle>(_context);
        var plateService = new PlateNormalizationService();
        _service = new VehicleService(repository, plateService, new LoggerConfiguration().CreateLogger());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateVehicle()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var result = await _service.CreateAsync(new CreateVehicleDto
        {
            PlateNumber = "AB-123-CD", Brand = "BMW", Model = "3 Serie", Year = 2021,
            FuelType = FuelType.Diesel, TransmissionType = TransmissionType.Automaat,
            Mileage = 50000, CustomerId = customer.Id
        });

        Assert.NotEqual(0, result.Id);
        Assert.Equal("AB-123-CD", result.PlateNumberOriginal);
    }

    [Fact]
    public async Task SearchByPlateAsync_ShouldFindNormalized()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        _context.Vehicles.Add(new Vehicle { PlateNumberOriginal = "AB-123-CD", PlateNumberNormalized = "AB123CD", Brand = "VW", Model = "Golf", Year = 2020, CustomerId = customer.Id });
        await _context.SaveChangesAsync();

        var result = await _service.SearchByPlateAsync("ab 123 cd");
        Assert.Single(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteVehicle()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        var vehicle = new Vehicle { PlateNumberOriginal = "ZZ-999-AA", PlateNumberNormalized = "ZZ999AA", Brand = "Ford", Model = "Focus", Year = 2018, CustomerId = customer.Id };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(vehicle.Id);
        var deleted = await _context.Vehicles.IgnoreQueryFilters().FirstAsync(v => v.Id == vehicle.Id);
        Assert.False(deleted.IsActive);
    }

    public void Dispose() { _context.Dispose(); }
}
