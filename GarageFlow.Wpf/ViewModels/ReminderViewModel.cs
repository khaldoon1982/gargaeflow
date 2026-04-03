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

    public Array ReminderTypes => Enum.GetValues(typeof(ReminderType));

    public ReminderViewModel(IReminderService service, ICustomerService customerService, IVehicleService vehicleService, ILogger logger) { _service = service; _customerService = customerService; _vehicleService = vehicleService; _logger = logger; }

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
            ErrorMessage = null;
            await _service.CreateAsync(new CreateReminderDto { ReminderType = ReminderType, ReminderDate = ReminderDate, Message = Message, SendMethod = SendMethod.Intern, CustomerId = SelectedCustomerId, VehicleId = SelectedVehicleId });
            Message = string.Empty; await LoadDataAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task MarkAsSent()
    {
        if (SelectedReminder is null) return;
        try { await _service.MarkAsSentAsync(SelectedReminder.Id); await LoadDataAsync(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (SelectedReminder is null) return;
        try { await _service.CancelAsync(SelectedReminder.Id); await LoadDataAsync(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }
}
