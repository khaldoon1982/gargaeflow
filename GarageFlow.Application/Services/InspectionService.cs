using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Application.Services;

public class InspectionService : IInspectionService
{
    private readonly IRepository<Inspection> _repo;
    private readonly ILogger _logger;

    public InspectionService(IRepository<Inspection> repo, ILogger logger) { _repo = repo; _logger = logger; }

    public async Task<IEnumerable<InspectionDto>> GetAllAsync() => (await _repo.GetAllAsync()).Select(MapToDto);

    public async Task<InspectionDto?> GetByIdAsync(int id) { var i = await _repo.GetByIdAsync(id); return i is null ? null : MapToDto(i); }

    public async Task<IEnumerable<InspectionDto>> GetByVehicleIdAsync(int vehicleId) => (await _repo.FindAsync(i => i.VehicleId == vehicleId)).Select(MapToDto);

    public async Task<IEnumerable<InspectionDto>> GetExpiredAsync() => (await _repo.FindAsync(i => i.ExpiryDate < DateTime.Today && i.Status != InspectionStatus.Goedgekeurd)).Select(MapToDto);

    public async Task<IEnumerable<InspectionDto>> GetDueSoonAsync(int days = 30) => (await _repo.FindAsync(i => i.ExpiryDate <= DateTime.Today.AddDays(days) && i.ExpiryDate >= DateTime.Today)).Select(MapToDto);

    public async Task<InspectionDto> CreateAsync(CreateInspectionDto dto)
    {
        var entity = new Inspection { InspectionType = dto.InspectionType, InspectionDate = dto.InspectionDate, ExpiryDate = dto.ExpiryDate, ReminderDate = dto.ReminderDate, Notes = dto.Notes, VehicleId = dto.VehicleId };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        _logger.Information("Keuring aangemaakt: {Id}", created.Id);
        return MapToDto(created);
    }

    public async Task<InspectionDto> UpdateAsync(UpdateInspectionDto dto)
    {
        var i = await _repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Keuring met Id {dto.Id} niet gevonden.");
        i.InspectionType = dto.InspectionType; i.InspectionDate = dto.InspectionDate; i.ExpiryDate = dto.ExpiryDate;
        i.ReminderDate = dto.ReminderDate; i.Status = dto.Status; i.Notes = dto.Notes; i.VehicleId = dto.VehicleId;
        await _repo.UpdateAsync(i);
        await _repo.SaveChangesAsync();
        return MapToDto(i);
    }

    public async Task DeleteAsync(int id)
    {
        var i = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Keuring met Id {id} niet gevonden.");
        i.IsActive = false;
        await _repo.UpdateAsync(i);
        await _repo.SaveChangesAsync();
    }

    private static InspectionDto MapToDto(Inspection i) => new()
    {
        Id = i.Id, InspectionType = i.InspectionType, InspectionDate = i.InspectionDate, ExpiryDate = i.ExpiryDate,
        ReminderDate = i.ReminderDate, Status = i.Status, Notes = i.Notes, VehicleId = i.VehicleId,
        VehiclePlate = i.Vehicle?.PlateNumberOriginal ?? string.Empty,
        CustomerName = i.Vehicle?.Customer?.DisplayName ?? string.Empty
    };
}
