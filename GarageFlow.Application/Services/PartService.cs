using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using Serilog;

namespace GarageFlow.Application.Services;

public class PartService : IPartService
{
    private readonly IRepository<Part> _repo;
    private readonly ILogger _logger;

    public PartService(IRepository<Part> repo, ILogger logger) { _repo = repo; _logger = logger; }

    public async Task<IEnumerable<PartDto>> GetByMaintenanceRecordIdAsync(int maintenanceRecordId)
    {
        var parts = await _repo.FindAsync(p => p.MaintenanceRecordId == maintenanceRecordId);
        return parts.Select(p => new PartDto { Id = p.Id, Name = p.Name, PartNumber = p.PartNumber, Quantity = p.Quantity, UnitPrice = p.UnitPrice, TotalPrice = p.TotalPrice, MaintenanceRecordId = p.MaintenanceRecordId });
    }

    public async Task<PartDto> CreateAsync(CreatePartDto dto)
    {
        var entity = new Part { Name = dto.Name, PartNumber = dto.PartNumber, Quantity = dto.Quantity, UnitPrice = dto.UnitPrice, TotalPrice = dto.Quantity * dto.UnitPrice, MaintenanceRecordId = dto.MaintenanceRecordId };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        _logger.Information("Onderdeel toegevoegd: {Name}", created.Name);
        return new PartDto { Id = created.Id, Name = created.Name, PartNumber = created.PartNumber, Quantity = created.Quantity, UnitPrice = created.UnitPrice, TotalPrice = created.TotalPrice, MaintenanceRecordId = created.MaintenanceRecordId };
    }

    public async Task DeleteAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Onderdeel met Id {id} niet gevonden.");
        p.IsActive = false;
        await _repo.UpdateAsync(p);
        await _repo.SaveChangesAsync();
    }
}
