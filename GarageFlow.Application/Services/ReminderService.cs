using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Application.Services;

public class ReminderService : IReminderService
{
    private readonly IRepository<Reminder> _repo;
    private readonly ILogger _logger;

    public ReminderService(IRepository<Reminder> repo, ILogger logger) { _repo = repo; _logger = logger; }

    public async Task<IEnumerable<ReminderDto>> GetAllAsync() => (await _repo.GetAllAsync()).Select(MapToDto);

    public async Task<IEnumerable<ReminderDto>> GetOpenAsync() => (await _repo.FindAsync(r => r.Status == ReminderStatus.Openstaand)).Select(MapToDto);

    public async Task<ReminderDto> CreateAsync(CreateReminderDto dto)
    {
        var entity = new Reminder { ReminderType = dto.ReminderType, ReminderDate = dto.ReminderDate, Message = dto.Message, SendMethod = dto.SendMethod, CustomerId = dto.CustomerId, VehicleId = dto.VehicleId };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        _logger.Information("Herinnering aangemaakt: {Id}", created.Id);
        return MapToDto(created);
    }

    public async Task MarkAsSentAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Herinnering met Id {id} niet gevonden.");
        r.IsSent = true; r.SentAt = DateTime.UtcNow; r.Status = ReminderStatus.Verzonden;
        await _repo.UpdateAsync(r);
        await _repo.SaveChangesAsync();
    }

    public async Task CancelAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Herinnering met Id {id} niet gevonden.");
        r.Status = ReminderStatus.Geannuleerd;
        await _repo.UpdateAsync(r);
        await _repo.SaveChangesAsync();
    }

    private static ReminderDto MapToDto(Reminder r) => new()
    {
        Id = r.Id, ReminderType = r.ReminderType, ReminderDate = r.ReminderDate, Message = r.Message,
        SendMethod = r.SendMethod, IsSent = r.IsSent, SentAt = r.SentAt, Status = r.Status,
        CustomerId = r.CustomerId, CustomerName = r.Customer?.DisplayName ?? string.Empty,
        VehicleId = r.VehicleId, VehiclePlate = r.Vehicle?.PlateNumberOriginal
    };
}
