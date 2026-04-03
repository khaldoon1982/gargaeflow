using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IReminderService
{
    Task<IEnumerable<ReminderDto>> GetAllAsync();
    Task<IEnumerable<ReminderDto>> GetOpenAsync();
    Task<ReminderDto> CreateAsync(CreateReminderDto dto);
    Task MarkAsSentAsync(int id);
    Task CancelAsync(int id);
}
