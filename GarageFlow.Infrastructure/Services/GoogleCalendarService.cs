using GarageFlow.Application.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Serilog;

namespace GarageFlow.Infrastructure.Services;

public class GoogleCalendarService : ICalendarService
{
    private readonly ILogger _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _calendarId;
    private CalendarService? _calendarService;

    public bool IsConfigured { get; }
    public bool IsAuthorized => _calendarService is not null;

    public GoogleCalendarService(ILogger logger)
    {
        _logger = logger;
        _clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
        _clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
        _calendarId = Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_ID") ?? "primary";

        IsConfigured = !string.IsNullOrEmpty(_clientId) && !string.IsNullOrEmpty(_clientSecret);
    }

    public async Task<bool> AuthorizeAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.Warning("Google Calendar is niet geconfigureerd (GOOGLE_CLIENT_ID/SECRET ontbreekt)");
            return false;
        }

        try
        {
            var tokenPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GarageFlow", "google-tokens");

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
                new[] { CalendarService.Scope.Calendar },
                "garageflow-user",
                ct,
                new FileDataStore(tokenPath, true));

            _calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GarageFlow"
            });

            _logger.Information("Google Calendar verbonden");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Google Calendar autorisatie mislukt");
            return false;
        }
    }

    public async Task<string?> CreateEventAsync(CalendarEventDto eventDto, CancellationToken ct = default)
    {
        if (_calendarService is null) return null;

        try
        {
            var calendarEvent = new Event
            {
                Summary = eventDto.Title,
                Description = eventDto.Description,
                Location = eventDto.Location,
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(eventDto.StartTime), TimeZone = "Europe/Amsterdam" },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(eventDto.EndTime), TimeZone = "Europe/Amsterdam" },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new() { Method = "popup", Minutes = 60 },
                        new() { Method = "email", Minutes = 1440 }
                    }
                }
            };

            var request = _calendarService.Events.Insert(calendarEvent, _calendarId);
            var created = await request.ExecuteAsync(ct);

            _logger.Information("Google Calendar event aangemaakt: {EventId} - {Title}", created.Id, eventDto.Title);
            return created.Id;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fout bij aanmaken Google Calendar event: {Title}", eventDto.Title);
            return null;
        }
    }

    public async Task<bool> UpdateEventAsync(string eventId, CalendarEventDto eventDto, CancellationToken ct = default)
    {
        if (_calendarService is null) return false;

        try
        {
            var existing = await _calendarService.Events.Get(_calendarId, eventId).ExecuteAsync(ct);
            existing.Summary = eventDto.Title;
            existing.Description = eventDto.Description;
            existing.Location = eventDto.Location;
            existing.Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(eventDto.StartTime), TimeZone = "Europe/Amsterdam" };
            existing.End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(eventDto.EndTime), TimeZone = "Europe/Amsterdam" };

            await _calendarService.Events.Update(existing, _calendarId, eventId).ExecuteAsync(ct);
            _logger.Information("Google Calendar event bijgewerkt: {EventId}", eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fout bij bijwerken Google Calendar event: {EventId}", eventId);
            return false;
        }
    }

    public async Task<bool> DeleteEventAsync(string eventId, CancellationToken ct = default)
    {
        if (_calendarService is null) return false;

        try
        {
            await _calendarService.Events.Delete(_calendarId, eventId).ExecuteAsync(ct);
            _logger.Information("Google Calendar event verwijderd: {EventId}", eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fout bij verwijderen Google Calendar event: {EventId}", eventId);
            return false;
        }
    }

    public async Task<IEnumerable<CalendarEventDto>> GetUpcomingEventsAsync(int days = 30, CancellationToken ct = default)
    {
        if (_calendarService is null) return Enumerable.Empty<CalendarEventDto>();

        try
        {
            var request = _calendarService.Events.List(_calendarId);
            request.TimeMinDateTimeOffset = DateTimeOffset.Now;
            request.TimeMaxDateTimeOffset = DateTimeOffset.Now.AddDays(days);
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.MaxResults = 50;

            var events = await request.ExecuteAsync(ct);

            return events.Items?.Select(e => new CalendarEventDto
            {
                EventId = e.Id,
                Title = e.Summary ?? "",
                Description = e.Description,
                StartTime = e.Start?.DateTimeDateTimeOffset?.LocalDateTime ?? DateTime.Today,
                EndTime = e.End?.DateTimeDateTimeOffset?.LocalDateTime ?? DateTime.Today,
                Location = e.Location
            }) ?? Enumerable.Empty<CalendarEventDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fout bij ophalen Google Calendar events");
            return Enumerable.Empty<CalendarEventDto>();
        }
    }
}
