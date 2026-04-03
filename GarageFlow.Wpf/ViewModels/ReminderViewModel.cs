using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class ReminderViewModel : ObservableObject
{
    private readonly IReminderService _service;
    private readonly ICustomerService _customerService;
    private readonly IVehicleService _vehicleService;
    private readonly IEmailService _emailService;
    private readonly ICalendarService _calendarService;
    private readonly ILogger _logger;

    [ObservableProperty] private ObservableCollection<ReminderDto> _reminders = new();
    [ObservableProperty] private ObservableCollection<CustomerDto> _customers = new();
    [ObservableProperty] private ObservableCollection<VehicleDto> _vehicles = new();
    [ObservableProperty] private ReminderDto? _selectedReminder;
    [ObservableProperty] private ReminderType _reminderType;
    [ObservableProperty] private DateTime _reminderDate = DateTime.Today;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private int _selectedCustomerId;
    [ObservableProperty] private int? _selectedVehicleId;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private bool _emailConfigured;
    [ObservableProperty] private bool _calendarConfigured;

    public Array ReminderTypes => Enum.GetValues(typeof(ReminderType));

    public ReminderViewModel(IReminderService service, ICustomerService customerService, IVehicleService vehicleService,
        IEmailService emailService, ICalendarService calendarService, ILogger logger)
    {
        _service = service; _customerService = customerService; _vehicleService = vehicleService;
        _emailService = emailService; _calendarService = calendarService; _logger = logger;
        EmailConfigured = emailService.IsConfigured;
        CalendarConfigured = calendarService.IsConfigured;
    }

    public async Task LoadDataAsync()
    {
        Reminders = new ObservableCollection<ReminderDto>(await _service.GetAllAsync());
        Customers = new ObservableCollection<CustomerDto>(await _customerService.GetAllAsync());
        Vehicles = new ObservableCollection<VehicleDto>(await _vehicleService.GetAllAsync());
    }

    [RelayCommand]
    private async Task Create()
    {
        try
        {
            ErrorMessage = null; StatusMessage = null;
            await _service.CreateAsync(new CreateReminderDto { ReminderType = ReminderType, ReminderDate = ReminderDate, Message = Message, SendMethod = SendMethod.Intern, CustomerId = SelectedCustomerId, VehicleId = SelectedVehicleId });
            Message = string.Empty; await LoadDataAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task MarkAsSent()
    {
        if (SelectedReminder is null) return;
        try { await _service.MarkAsSentAsync(SelectedReminder.Id); await LoadDataAsync(); StatusMessage = "Herinnering gemarkeerd als verzonden."; }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (SelectedReminder is null) return;
        try { await _service.CancelAsync(SelectedReminder.Id); await LoadDataAsync(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task SendEmail()
    {
        if (SelectedReminder is null) return;
        try
        {
            ErrorMessage = null; StatusMessage = null;
            var success = await _emailService.SendReminderEmailAsync(SelectedReminder.Id);
            if (success)
            {
                await _service.MarkAsSentAsync(SelectedReminder.Id);
                await LoadDataAsync();
                StatusMessage = "E-mail succesvol verzonden!";
            }
            else
            {
                ErrorMessage = "E-mail kon niet worden verzonden. Controleer de Resend configuratie.";
            }
        }
        catch (Exception ex) { ErrorMessage = ex.Message; _logger.Error(ex, "Fout bij verzenden e-mail"); }
    }

    [RelayCommand]
    private async Task AddToCalendar()
    {
        if (SelectedReminder is null) return;
        try
        {
            ErrorMessage = null; StatusMessage = null;

            if (!_calendarService.IsAuthorized)
            {
                var authorized = await _calendarService.AuthorizeAsync();
                if (!authorized)
                {
                    ErrorMessage = "Google Calendar autorisatie mislukt. Controleer de configuratie.";
                    return;
                }
            }

            var eventDto = new CalendarEventDto
            {
                Title = $"GarageFlow: {SelectedReminder.Message}",
                Description = $"Klant: {SelectedReminder.CustomerName}\nKenteken: {SelectedReminder.VehiclePlate ?? "N.v.t."}\nType: {SelectedReminder.ReminderType}",
                StartTime = SelectedReminder.ReminderDate.Date.AddHours(9),
                EndTime = SelectedReminder.ReminderDate.Date.AddHours(10),
                ReminderId = SelectedReminder.Id,
                CustomerId = SelectedReminder.CustomerId
            };

            var eventId = await _calendarService.CreateEventAsync(eventDto);
            if (eventId is not null)
                StatusMessage = "Afspraak toegevoegd aan Google Calendar!";
            else
                ErrorMessage = "Kon afspraak niet toevoegen aan Google Calendar.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; _logger.Error(ex, "Fout bij toevoegen aan agenda"); }
    }

    [RelayCommand]
    private async Task ConnectCalendar()
    {
        try
        {
            ErrorMessage = null; StatusMessage = null;
            var success = await _calendarService.AuthorizeAsync();
            CalendarConfigured = _calendarService.IsAuthorized;
            StatusMessage = success ? "Google Calendar verbonden!" : "Verbinding mislukt.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }
}
