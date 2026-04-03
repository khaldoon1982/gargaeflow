using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class MaintenanceViewModel : ObservableObject
{
    private readonly IMaintenanceRecordService _service;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger _logger;

    [ObservableProperty] private ObservableCollection<MaintenanceRecordDto> _records = new();
    [ObservableProperty] private ObservableCollection<VehicleDto> _vehicles = new();
    [ObservableProperty] private MaintenanceRecordDto? _selectedRecord;
    [ObservableProperty] private DateTime _serviceDate = DateTime.Today;
    [ObservableProperty] private int _mileageAtService;
    [ObservableProperty] private string _serviceType = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string? _technicianName;
    [ObservableProperty] private decimal _laborCost;
    [ObservableProperty] private decimal _partsCost;
    [ObservableProperty] private DateTime? _nextServiceDate;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private int _selectedVehicleId;
    [ObservableProperty] private MaintenanceStatus _status;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string? _errorMessage;

    public Array Statuses => Enum.GetValues(typeof(MaintenanceStatus));

    public MaintenanceViewModel(IMaintenanceRecordService service, IVehicleService vehicleService, ILogger logger) { _service = service; _vehicleService = vehicleService; _logger = logger; }

    public async Task LoadDataAsync()
    {
        Records = new ObservableCollection<MaintenanceRecordDto>(await _service.GetAllAsync());
        Vehicles = new ObservableCollection<VehicleDto>(await _vehicleService.GetAllAsync());
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            ErrorMessage = null;
            if (IsEditing && SelectedRecord is not null)
                await _service.UpdateAsync(new UpdateMaintenanceRecordDto { Id = SelectedRecord.Id, ServiceDate = ServiceDate, MileageAtService = MileageAtService, ServiceType = ServiceType, Description = Description, TechnicianName = TechnicianName, LaborCost = LaborCost, PartsCost = PartsCost, NextServiceDate = NextServiceDate, Notes = Notes, VehicleId = SelectedVehicleId, Status = Status });
            else
                await _service.CreateAsync(new CreateMaintenanceRecordDto { ServiceDate = ServiceDate, MileageAtService = MileageAtService, ServiceType = ServiceType, Description = Description, TechnicianName = TechnicianName, LaborCost = LaborCost, PartsCost = PartsCost, NextServiceDate = NextServiceDate, Notes = Notes, VehicleId = SelectedVehicleId });
            ClearForm(); await LoadDataAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; _logger.Error(ex, "Fout bij opslaan onderhoud"); }
    }

    [RelayCommand] private async Task Delete() { if (SelectedRecord is null) return; try { await _service.DeleteAsync(SelectedRecord.Id); ClearForm(); await LoadDataAsync(); } catch (Exception ex) { ErrorMessage = ex.Message; } }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedRecord is null) return;
        ServiceDate = SelectedRecord.ServiceDate; MileageAtService = SelectedRecord.MileageAtService;
        ServiceType = SelectedRecord.ServiceType; Description = SelectedRecord.Description;
        TechnicianName = SelectedRecord.TechnicianName; LaborCost = SelectedRecord.LaborCost;
        PartsCost = SelectedRecord.PartsCost; NextServiceDate = SelectedRecord.NextServiceDate;
        Notes = SelectedRecord.Notes; SelectedVehicleId = SelectedRecord.VehicleId;
        Status = SelectedRecord.Status; IsEditing = true;
    }

    [RelayCommand]
    private void ClearForm()
    {
        ServiceDate = DateTime.Today; MileageAtService = 0; ServiceType = string.Empty; Description = string.Empty;
        TechnicianName = null; LaborCost = 0; PartsCost = 0; NextServiceDate = null; Notes = null;
        SelectedVehicleId = 0; Status = MaintenanceStatus.Gepland; IsEditing = false; SelectedRecord = null; ErrorMessage = null;
    }
}
