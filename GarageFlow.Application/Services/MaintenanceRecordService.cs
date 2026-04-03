using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Application.Services;

public class MaintenanceRecordService : IMaintenanceRecordService
{
    private readonly IRepository<MaintenanceRecord> _repo;
    private readonly ILogger _logger;

    public MaintenanceRecordService(IRepository<MaintenanceRecord> repo, ILogger logger) { _repo = repo; _logger = logger; }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetAllAsync() => (await _repo.GetAllAsync()).Select(MapToDto);

    public async Task<MaintenanceRecordDto?> GetByIdAsync(int id)
    {
        var m = await _repo.GetByIdAsync(id);
        return m is null ? null : MapToDto(m);
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetByVehicleIdAsync(int vehicleId) => (await _repo.FindAsync(m => m.VehicleId == vehicleId)).Select(MapToDto);

    public async Task<MaintenanceRecordDto> CreateAsync(CreateMaintenanceRecordDto dto)
    {
        var entity = new MaintenanceRecord
        {
            ServiceDate = dto.ServiceDate, MileageAtService = dto.MileageAtService, ServiceType = dto.ServiceType,
            Description = dto.Description, PartsChanged = dto.PartsChanged,
            LaborCost = dto.LaborCost, PartsCost = dto.PartsCost, TotalCost = dto.LaborCost + dto.PartsCost,
            TechnicianName = dto.TechnicianName, NextServiceDate = dto.NextServiceDate, NextServiceMileage = dto.NextServiceMileage,
            Notes = dto.Notes, VehicleId = dto.VehicleId, Status = MaintenanceStatus.Gepland
        };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        _logger.Information("Onderhoudsrecord aangemaakt: {Id}", created.Id);
        return MapToDto(created);
    }

    public async Task<MaintenanceRecordDto> UpdateAsync(UpdateMaintenanceRecordDto dto)
    {
        var m = await _repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Onderhoudsrecord met Id {dto.Id} niet gevonden.");
        m.ServiceDate = dto.ServiceDate; m.MileageAtService = dto.MileageAtService; m.ServiceType = dto.ServiceType;
        m.Description = dto.Description; m.PartsChanged = dto.PartsChanged;
        m.LaborCost = dto.LaborCost; m.PartsCost = dto.PartsCost; m.TotalCost = dto.LaborCost + dto.PartsCost;
        m.TechnicianName = dto.TechnicianName; m.NextServiceDate = dto.NextServiceDate; m.NextServiceMileage = dto.NextServiceMileage;
        m.Notes = dto.Notes; m.VehicleId = dto.VehicleId; m.Status = dto.Status;
        await _repo.UpdateAsync(m);
        await _repo.SaveChangesAsync();
        return MapToDto(m);
    }

    public async Task DeleteAsync(int id)
    {
        var m = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Onderhoudsrecord met Id {id} niet gevonden.");
        m.IsActive = false;
        await _repo.UpdateAsync(m);
        await _repo.SaveChangesAsync();
    }

    private static MaintenanceRecordDto MapToDto(MaintenanceRecord m) => new()
    {
        Id = m.Id, ServiceDate = m.ServiceDate, MileageAtService = m.MileageAtService, ServiceType = m.ServiceType,
        Description = m.Description, PartsChanged = m.PartsChanged, LaborCost = m.LaborCost, PartsCost = m.PartsCost,
        TotalCost = m.TotalCost, TechnicianName = m.TechnicianName, NextServiceDate = m.NextServiceDate,
        NextServiceMileage = m.NextServiceMileage, Status = m.Status, Notes = m.Notes,
        VehicleId = m.VehicleId,
        VehiclePlate = m.Vehicle?.PlateNumberOriginal ?? string.Empty,
        CustomerName = m.Vehicle?.Customer?.DisplayName ?? string.Empty
    };
}
