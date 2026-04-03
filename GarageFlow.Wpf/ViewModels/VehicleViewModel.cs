using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class VehicleViewModel : ObservableObject
{
    private readonly IVehicleService _vehicleService;
    private readonly ICustomerService _customerService;
    private readonly ILogger _logger;

    [ObservableProperty] private ObservableCollection<VehicleDto> _vehicles = new();
    [ObservableProperty] private ObservableCollection<CustomerDto> _customers = new();
    [ObservableProperty] private VehicleDto? _selectedVehicle;
    [ObservableProperty] private string _plateNumber = string.Empty;
    [ObservableProperty] private string _brand = string.Empty;
    [ObservableProperty] private string _model = string.Empty;
    [ObservableProperty] private int _year = DateTime.Now.Year;
    [ObservableProperty] private FuelType _fuelType;
    [ObservableProperty] private TransmissionType _transmissionType;
    [ObservableProperty] private int _mileage;
    [ObservableProperty] private string? _color;
    [ObservableProperty] private string? _vIN;
    [ObservableProperty] private string? _chassisNumber;
    [ObservableProperty] private int _selectedCustomerId;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string? _errorMessage;

    public Array FuelTypes => Enum.GetValues(typeof(FuelType));
    public Array TransmissionTypes => Enum.GetValues(typeof(TransmissionType));

    public VehicleViewModel(IVehicleService vehicleService, ICustomerService customerService, ILogger logger) { _vehicleService = vehicleService; _customerService = customerService; _logger = logger; }

    public async Task LoadDataAsync()
    {
        Vehicles = new ObservableCollection<VehicleDto>(await _vehicleService.GetAllAsync());
        Customers = new ObservableCollection<CustomerDto>(await _customerService.GetAllAsync());
    }

    [RelayCommand]
    private async Task SearchByPlate()
    {
        var items = string.IsNullOrWhiteSpace(SearchText)
            ? await _vehicleService.GetAllAsync()
            : await _vehicleService.SearchByPlateAsync(SearchText);
        Vehicles = new ObservableCollection<VehicleDto>(items);
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            ErrorMessage = null;
            if (IsEditing && SelectedVehicle is not null)
                await _vehicleService.UpdateAsync(new UpdateVehicleDto { Id = SelectedVehicle.Id, PlateNumber = PlateNumber, Brand = Brand, Model = Model, Year = Year, FuelType = FuelType, TransmissionType = TransmissionType, Mileage = Mileage, Color = Color, VIN = VIN, ChassisNumber = ChassisNumber, CustomerId = SelectedCustomerId });
            else
                await _vehicleService.CreateAsync(new CreateVehicleDto { PlateNumber = PlateNumber, Brand = Brand, Model = Model, Year = Year, FuelType = FuelType, TransmissionType = TransmissionType, Mileage = Mileage, Color = Color, VIN = VIN, ChassisNumber = ChassisNumber, CustomerId = SelectedCustomerId });
            ClearForm();
            await LoadDataAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; _logger.Error(ex, "Fout bij opslaan voertuig"); }
    }

    [RelayCommand] private async Task Delete() { if (SelectedVehicle is null) return; try { await _vehicleService.DeleteAsync(SelectedVehicle.Id); ClearForm(); await LoadDataAsync(); } catch (Exception ex) { ErrorMessage = ex.Message; } }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedVehicle is null) return;
        PlateNumber = SelectedVehicle.PlateNumberOriginal; Brand = SelectedVehicle.Brand; Model = SelectedVehicle.Model;
        Year = SelectedVehicle.Year; FuelType = SelectedVehicle.FuelType; TransmissionType = SelectedVehicle.TransmissionType;
        Mileage = SelectedVehicle.Mileage; Color = SelectedVehicle.Color; VIN = SelectedVehicle.VIN;
        ChassisNumber = SelectedVehicle.ChassisNumber; SelectedCustomerId = SelectedVehicle.CustomerId; IsEditing = true;
    }

    [RelayCommand]
    private void ClearForm()
    {
        PlateNumber = string.Empty; Brand = string.Empty; Model = string.Empty; Year = DateTime.Now.Year;
        FuelType = FuelType.Benzine; TransmissionType = TransmissionType.Handgeschakeld; Mileage = 0;
        Color = null; VIN = null; ChassisNumber = null; SelectedCustomerId = 0;
        IsEditing = false; SelectedVehicle = null; ErrorMessage = null;
    }
}
