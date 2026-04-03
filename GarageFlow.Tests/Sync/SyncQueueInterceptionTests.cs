using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using GarageFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GarageFlow.Tests.Sync;

public class SyncQueueInterceptionTests : IDisposable
{
    private readonly GarageFlowDbContext _context;

    public SyncQueueInterceptionTests()
    {
        var options = new DbContextOptionsBuilder<GarageFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new GarageFlowDbContext(options);
    }

    [Fact]
    public async Task AddingCustomer_ShouldCreateSyncQueueEntry()
    {
        _context.Customers.Add(new Customer
        {
            CustomerNumber = "KL-001", FirstName = "Test", LastName = "Klant", PhoneNumber = "06-1"
        });
        await _context.SaveChangesAsync();

        var queueEntries = await _context.SyncQueue.ToListAsync();
        Assert.Single(queueEntries);
        Assert.Equal("Customer", queueEntries[0].EntityName);
        Assert.Equal(SyncOperationType.Create, queueEntries[0].OperationType);
        Assert.Equal(SyncQueueStatus.Pending, queueEntries[0].Status);
    }

    [Fact]
    public async Task AddingVehicle_ShouldCreateSyncQueueEntry()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _context.Vehicles.Add(new Vehicle
        {
            PlateNumberOriginal = "AA-11-BB", PlateNumberNormalized = "AA11BB",
            Brand = "VW", Model = "Golf", Year = 2020, CustomerId = customer.Id
        });
        await _context.SaveChangesAsync();

        var vehicleQueue = await _context.SyncQueue.Where(s => s.EntityName == "Vehicle").ToListAsync();
        Assert.Single(vehicleQueue);
        Assert.Equal(SyncOperationType.Create, vehicleQueue[0].OperationType);
    }

    [Fact]
    public async Task SoftDeletingCustomer_ShouldCreateDeleteQueueEntry()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        customer.IsActive = false;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();

        var deleteEntries = await _context.SyncQueue.Where(s => s.OperationType == SyncOperationType.Delete).ToListAsync();
        Assert.Single(deleteEntries);
        Assert.Equal("Customer", deleteEntries[0].EntityName);
    }

    [Fact]
    public async Task SyncSuppressed_ShouldNotEnqueue()
    {
        _context.IsSyncSuppressed = true;

        _context.Customers.Add(new Customer
        {
            CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1"
        });
        await _context.SaveChangesAsync();

        _context.IsSyncSuppressed = false;

        var queueEntries = await _context.SyncQueue.ToListAsync();
        Assert.Empty(queueEntries);
    }

    [Fact]
    public async Task NewCustomer_ShouldHaveCloudIdAndPendingUpload()
    {
        var customer = new Customer { CustomerNumber = "KL-001", FirstName = "T", LastName = "K", PhoneNumber = "06-1" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        Assert.NotEqual(Guid.Empty, customer.CloudId);
        Assert.Equal(SyncStatus.PendingUpload, customer.SyncStatus);
        Assert.NotNull(customer.LastLocalChangeAtUtc);
        Assert.Equal(1, customer.VersionNumber);
    }

    [Fact]
    public async Task NonSyncableEntity_ShouldNotCreateQueueEntry()
    {
        _context.Set<AppSetting>().Add(new AppSetting { Key = "TestKey", Value = "TestValue" });
        await _context.SaveChangesAsync();

        var queueEntries = await _context.SyncQueue.ToListAsync();
        Assert.Empty(queueEntries);
    }

    public void Dispose() { _context.Dispose(); }
}
