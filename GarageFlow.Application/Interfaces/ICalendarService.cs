namespace GarageFlow.Application.Interfaces;

public interface ICalendarService
{
    Task<string?> CreateEventAsync(CalendarEventDto eventDto, CancellationToken ct = default);
    Task<bool> UpdateEventAsync(string eventId, CalendarEventDto eventDto, CancellationToken ct = default);
    Task<bool> DeleteEventAsync(string eventId, CancellationToken ct = default);
    Task<IEnumerable<CalendarEventDto>> GetUpcomingEventsAsync(int days = 30, CancellationToken ct = default);
    Task<bool> AuthorizeAsync(CancellationToken ct = default);
    bool IsAuthorized { get; }
    bool IsConfigured { get; }
}

public class CalendarEventDto
{
    public string? EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public int? ReminderId { get; set; }
    public int? CustomerId { get; set; }
    public int? VehicleId { get; set; }
}
