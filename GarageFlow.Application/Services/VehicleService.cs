using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using Serilog;

namespace GarageFlow.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IRepository<Vehicle> _repo;
    private readonly IPlateNormalizationService _plateService;
    private readonly ILogger _logger;

    public VehicleService(IRepository<Vehicle> repo, IPlateNormalizationService plateService, ILogger logger) { _repo = repo; _plateService = plateService; _logger = logger; }

    public async Task<IEnumerable<VehicleDto>> GetAllAsync() => (await _repo.GetAllAsync()).Select(MapToDto);

    public async Task<VehicleDto?> GetByIdAsync(int id)
    {
        var v = await _repo.GetByIdAsync(id);
        return v is null ? null : MapToDto(v);
    }

    public async Task<IEnumerable<VehicleDto>> GetByCustomerIdAsync(int customerId) => (await _repo.FindAsync(v => v.CustomerId == customerId)).Select(MapToDto);

    public async Task<IEnumerable<VehicleDto>> SearchByPlateAsync(string plateNumber)
    {
        var normalized = _plateService.Normalize(plateNumber);
        return (await _repo.FindAsync(v => v.PlateNumberNormalized.Contains(normalized))).Select(MapToDto);
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleDto dto)
    {
        var entity = new Vehicle
        {
            PlateNumberOriginal = dto.PlateNumber,
            PlateNumberNormalized = _plateService.Normalize(dto.PlateNumber),
            Brand = dto.Brand, Model = dto.Model, Trim = dto.Trim, Year = dto.Year,
            ChassisNumber = dto.ChassisNumber, EngineNumber = dto.EngineNumber, VIN = dto.VIN,
            FuelType = dto.FuelType, TransmissionType = dto.TransmissionType,
            Color = dto.Color, Mileage = dto.Mileage,
            InspectionDate = dto.InspectionDate, InspectionExpiryDate = dto.InspectionExpiryDate,
            LastServiceDate = dto.LastServiceDate, NextServiceDate = dto.NextServiceDate,
            Notes = dto.Notes, CustomerId = dto.CustomerId
        };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        _logger.Information("Voertuig aangemaakt: {Id} {Plate}", created.Id, created.PlateNumberOriginal);
        return MapToDto(created);
    }

    public async Task<VehicleDto> UpdateAsync(UpdateVehicleDto dto)
    {
        var v = await _repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Voertuig met Id {dto.Id} niet gevonden.");
        v.PlateNumberOriginal = dto.PlateNumber;
        v.PlateNumberNormalized = _plateService.Normalize(dto.PlateNumber);
        v.Brand = dto.Brand; v.Model = dto.Model; v.Trim = dto.Trim; v.Year = dto.Year;
        v.ChassisNumber = dto.ChassisNumber; v.EngineNumber = dto.EngineNumber; v.VIN = dto.VIN;
        v.FuelType = dto.FuelType; v.TransmissionType = dto.TransmissionType;
        v.Color = dto.Color; v.Mileage = dto.Mileage;
        v.InspectionDate = dto.InspectionDate; v.InspectionExpiryDate = dto.InspectionExpiryDate;
        v.LastServiceDate = dto.LastServiceDate; v.NextServiceDate = dto.NextServiceDate;
        v.Notes = dto.Notes; v.CustomerId = dto.CustomerId;
        await _repo.UpdateAsync(v);
        await _repo.SaveChangesAsync();
        return MapToDto(v);
    }

    public async Task DeleteAsync(int id)
    {
        var v = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Voertuig met Id {id} niet gevonden.");
        v.IsActive = false;
        await _repo.UpdateAsync(v);
        await _repo.SaveChangesAsync();
    }

    private static VehicleDto MapToDto(Vehicle v) => new()
    {
        Id = v.Id, PlateNumberOriginal = v.PlateNumberOriginal, Brand = v.Brand, Model = v.Model,
        Trim = v.Trim, Year = v.Year, ChassisNumber = v.ChassisNumber, EngineNumber = v.EngineNumber,
        VIN = v.VIN, FuelType = v.FuelType, TransmissionType = v.TransmissionType,
        Color = v.Color, Mileage = v.Mileage,
        InspectionDate = v.InspectionDate, InspectionExpiryDate = v.InspectionExpiryDate,
        LastServiceDate = v.LastServiceDate, NextServiceDate = v.NextServiceDate,
        Notes = v.Notes, IsArchived = v.IsArchived, CustomerId = v.CustomerId,
        CustomerName = v.Customer?.DisplayName ?? string.Empty
    };
}
