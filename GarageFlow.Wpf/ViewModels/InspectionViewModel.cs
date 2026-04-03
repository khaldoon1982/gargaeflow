using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class InspectionViewModel : ObservableObject
{
    private readonly IInspectionService _service;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger _logger;

    [ObservableProperty] private ObservableCollection<InspectionDto> _inspections = new();
    [ObservableProperty] private ObservableCollection<VehicleDto> _vehicles = new();
    [ObservableProperty] private InspectionDto? _selectedInspection;
    [ObservableProperty] private InspectionType _inspectionType;
    [ObservableProperty] private DateTime _inspectionDate = DateTime.Today;
    [ObservableProperty] private DateTime _expiryDate = DateTime.Today.AddYears(1);
    [ObservableProperty] private InspectionStatus _status;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private int _selectedVehicleId;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string? _errorMessage;

    public Array InspectionTypes => Enum.GetValues(typeof(InspectionType));
    public Array Statuses => Enum.GetValues(typeof(InspectionStatus));

    public InspectionViewModel(IInspectionService service, IVehicleService vehicleService, ILogger logger) { _service = service; _vehicleService = vehicleService; _logger = logger; }

    public async Task LoadDataAsync()
    {
        Inspections = new ObservableCollection<InspectionDto>(await _service.GetAllAsync());
        Vehicles = new ObservableCollection<VehicleDto>(await _vehicleService.GetAllAsync());
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            ErrorMessage = null;
            if (IsEditing && SelectedInspection is not null)
                await _service.UpdateAsync(new UpdateInspectionDto { Id = SelectedInspection.Id, InspectionType = InspectionType, InspectionDate = InspectionDate, ExpiryDate = ExpiryDate, Status = Status, Notes = Notes, VehicleId = SelectedVehicleId });
            else
                await _service.CreateAsync(new CreateInspectionDto { InspectionType = InspectionType, InspectionDate = InspectionDate, ExpiryDate = ExpiryDate, Notes = Notes, VehicleId = SelectedVehicleId });
            ClearForm(); await LoadDataAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; _logger.Error(ex, "Fout bij opslaan keuring"); }
    }

    [RelayCommand] private async Task Delete() { if (SelectedInspection is null) return; try { await _service.DeleteAsync(SelectedInspection.Id); ClearForm(); await LoadDataAsync(); } catch (Exception ex) { ErrorMessage = ex.Message; } }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedInspection is null) return;
        InspectionType = SelectedInspection.InspectionType; InspectionDate = SelectedInspection.InspectionDate;
        ExpiryDate = SelectedInspection.ExpiryDate; Status = SelectedInspection.Status;
        Notes = SelectedInspection.Notes; SelectedVehicleId = SelectedInspection.VehicleId; IsEditing = true;
    }

    [RelayCommand]
    private void ClearForm()
    {
        InspectionType = InspectionType.APK; InspectionDate = DateTime.Today; ExpiryDate = DateTime.Today.AddYears(1);
        Status = InspectionStatus.Gepland; Notes = null; SelectedVehicleId = 0;
        IsEditing = false; SelectedInspection = null; ErrorMessage = null;
    }
}
