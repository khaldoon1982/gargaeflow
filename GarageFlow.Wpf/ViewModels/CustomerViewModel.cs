using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Enums;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class CustomerViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IVehicleService _vehicleService;
    private readonly IMaintenanceRecordService _maintenanceService;
    private readonly IInspectionService _inspectionService;
    private readonly IReminderService _reminderService;
    private readonly ILogger _logger;

    // Left panel
    [ObservableProperty] private ObservableCollection<CustomerDto> _customers = new();
    [ObservableProperty] private CustomerDto? _selectedCustomer;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private CustomerType? _filterCustomerType;
    [ObservableProperty] private string? _filterStatus;

    // Right panel - form
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isNewCustomer;
    [ObservableProperty] private CustomerType _customerType = CustomerType.Private;
    [ObservableProperty] private string? _firstName;
    [ObservableProperty] private string? _lastName;
    [ObservableProperty] private string? _companyName;
    [ObservableProperty] private string? _contactPerson;
    [ObservableProperty] private string? _department;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string? _whatsAppNumber;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _billingEmail;
    [ObservableProperty] private PreferredContactMethod _preferredContactMethod;
    [ObservableProperty] private string? _street;
    [ObservableProperty] private string? _houseNumber;
    [ObservableProperty] private string? _houseNumberAddition;
    [ObservableProperty] private string? _postalCode;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _country = "Nederland";
    [ObservableProperty] private string? _chamberOfCommerceNumber;
    [ObservableProperty] private string? _vATNumber;
    [ObservableProperty] private PaymentTerm? _paymentTerm;
    [ObservableProperty] private string? _debtorNumber;
    [ObservableProperty] private string? _fleetManager;
    [ObservableProperty] private string? _billingNotes;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private bool _isArchived;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _customerNumber;

    // Tabs
    [ObservableProperty] private ObservableCollection<VehicleDto> _customerVehicles = new();
    [ObservableProperty] private ObservableCollection<MaintenanceRecordDto> _customerMaintenance = new();
    [ObservableProperty] private ObservableCollection<InspectionDto> _customerInspections = new();
    [ObservableProperty] private ObservableCollection<ReminderDto> _customerReminders = new();
    [ObservableProperty] private VehicleDto? _selectedVehicle;

    // Vehicle dialog
    [ObservableProperty] private bool _isVehicleDialogOpen;
    [ObservableProperty] private bool _isVehicleEditing;
    [ObservableProperty] private string _vehiclePlateNumber = string.Empty;
    [ObservableProperty] private string _vehicleBrand = string.Empty;
    [ObservableProperty] private string _vehicleModel = string.Empty;
    [ObservableProperty] private int _vehicleYear = DateTime.Now.Year;
    [ObservableProperty] private string? _vehicleChassisNumber;
    [ObservableProperty] private FuelType _vehicleFuelType;
    [ObservableProperty] private TransmissionType _vehicleTransmissionType;
    [ObservableProperty] private string? _vehicleColor;
    [ObservableProperty] private int _vehicleMileage;
    [ObservableProperty] private DateTime? _vehicleInspectionExpiryDate;
    [ObservableProperty] private string? _vehicleNotes;
    [ObservableProperty] private string? _vehicleErrorMessage;

    public bool IsBusiness => CustomerType == CustomerType.Business;
    public Array FuelTypes => Enum.GetValues(typeof(FuelType));
    public Array TransmissionTypes => Enum.GetValues(typeof(TransmissionType));
    public Array ContactMethods => Enum.GetValues(typeof(PreferredContactMethod));
    public Array PaymentTerms => Enum.GetValues(typeof(PaymentTerm));

    public CustomerViewModel(ICustomerService customerService, IVehicleService vehicleService,
        IMaintenanceRecordService maintenanceService, IInspectionService inspectionService,
        IReminderService reminderService, ILogger logger)
    {
        _customerService = customerService; _vehicleService = vehicleService;
        _maintenanceService = maintenanceService; _inspectionService = inspectionService;
        _reminderService = reminderService; _logger = logger;
    }

    partial void OnCustomerTypeChanged(CustomerType value) => OnPropertyChanged(nameof(IsBusiness));

    public async Task LoadDataAsync() => await RefreshList();

    private async Task RefreshList()
    {
        var items = string.IsNullOrWhiteSpace(SearchText)
            ? await _customerService.GetAllAsync()
            : await _customerService.SearchAsync(SearchText);

        if (FilterCustomerType.HasValue)
            items = items.Where(c => c.CustomerType == FilterCustomerType.Value);
        if (FilterStatus == "Actief") items = items.Where(c => !c.IsArchived);
        else if (FilterStatus == "Gearchiveerd") items = items.Where(c => c.IsArchived);

        Customers = new ObservableCollection<CustomerDto>(items);
    }

    partial void OnSelectedCustomerChanged(CustomerDto? value)
    {
        if (value is null) return;
        LoadDetail(value);
        _ = LoadTabsAsync(value.Id);
    }

    private void LoadDetail(CustomerDto c)
    {
        IsNewCustomer = false; IsEditing = false; CustomerNumber = c.CustomerNumber;
        CustomerType = c.CustomerType; FirstName = c.FirstName; LastName = c.LastName;
        CompanyName = c.CompanyName; ContactPerson = c.ContactPerson; Department = c.Department;
        PhoneNumber = c.PhoneNumber; WhatsAppNumber = c.WhatsAppNumber; Email = c.Email;
        BillingEmail = c.BillingEmail; PreferredContactMethod = c.PreferredContactMethod;
        Street = c.Street; HouseNumber = c.HouseNumber; HouseNumberAddition = c.HouseNumberAddition;
        PostalCode = c.PostalCode; City = c.City; Country = c.Country;
        ChamberOfCommerceNumber = c.ChamberOfCommerceNumber; VATNumber = c.VATNumber;
        PaymentTerm = c.PaymentTerm; DebtorNumber = c.DebtorNumber; FleetManager = c.FleetManager;
        BillingNotes = c.BillingNotes; Notes = c.Notes; IsArchived = c.IsArchived; ErrorMessage = null;
    }

    private async Task LoadTabsAsync(int customerId)
    {
        var vehicles = await _vehicleService.GetByCustomerIdAsync(customerId);
        CustomerVehicles = new ObservableCollection<VehicleDto>(vehicles);
        var vIds = CustomerVehicles.Select(v => v.Id).ToHashSet();

        var allM = await _maintenanceService.GetAllAsync();
        CustomerMaintenance = new ObservableCollection<MaintenanceRecordDto>(allM.Where(m => vIds.Contains(m.VehicleId)));
        var allI = await _inspectionService.GetAllAsync();
        CustomerInspections = new ObservableCollection<InspectionDto>(allI.Where(i => vIds.Contains(i.VehicleId)));
        var allR = await _reminderService.GetAllAsync();
        CustomerReminders = new ObservableCollection<ReminderDto>(allR.Where(r => r.CustomerId == customerId));
    }

    [RelayCommand] private async Task Search() => await RefreshList();
    [RelayCommand] private async Task Refresh() { SearchText = string.Empty; FilterCustomerType = null; FilterStatus = null; await RefreshList(); }

    [RelayCommand]
    private void NewCustomer()
    {
        SelectedCustomer = null; IsNewCustomer = true; IsEditing = true;
        CustomerNumber = "(nieuw)"; CustomerType = CustomerType.Private;
        FirstName = null; LastName = null; CompanyName = null; ContactPerson = null; Department = null;
        PhoneNumber = string.Empty; WhatsAppNumber = null; Email = null; BillingEmail = null;
        PreferredContactMethod = PreferredContactMethod.Phone;
        Street = null; HouseNumber = null; HouseNumberAddition = null; PostalCode = null; City = null; Country = "Nederland";
        ChamberOfCommerceNumber = null; VATNumber = null; PaymentTerm = null; DebtorNumber = null; FleetManager = null;
        BillingNotes = null; Notes = null; IsArchived = false; ErrorMessage = null;
        CustomerVehicles = new(); CustomerMaintenance = new(); CustomerInspections = new(); CustomerReminders = new();
    }

    [RelayCommand] private void Edit() => IsEditing = true;

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            ErrorMessage = null;
            if (IsNewCustomer)
            {
                var created = await _customerService.CreateAsync(new CreateCustomerDto
                {
                    CustomerType = CustomerType, FirstName = FirstName, LastName = LastName,
                    CompanyName = CompanyName, ContactPerson = ContactPerson, Department = Department,
                    PhoneNumber = PhoneNumber, WhatsAppNumber = WhatsAppNumber, Email = Email,
                    BillingEmail = BillingEmail, PreferredContactMethod = PreferredContactMethod,
                    Street = Street, HouseNumber = HouseNumber, HouseNumberAddition = HouseNumberAddition,
                    PostalCode = PostalCode, City = City, Country = Country,
                    ChamberOfCommerceNumber = ChamberOfCommerceNumber, VATNumber = VATNumber,
                    PaymentTerm = PaymentTerm, DebtorNumber = DebtorNumber, FleetManager = FleetManager,
                    BillingNotes = BillingNotes, Notes = Notes
                });
                IsNewCustomer = false;
                await RefreshList();
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == created.Id);
            }
            else if (SelectedCustomer is not null)
            {
                await _customerService.UpdateAsync(new UpdateCustomerDto
                {
                    Id = SelectedCustomer.Id, CustomerType = CustomerType, FirstName = FirstName, LastName = LastName,
                    CompanyName = CompanyName, ContactPerson = ContactPerson, Department = Department,
                    PhoneNumber = PhoneNumber, WhatsAppNumber = WhatsAppNumber, Email = Email,
                    BillingEmail = BillingEmail, PreferredContactMethod = PreferredContactMethod,
                    Street = Street, HouseNumber = HouseNumber, HouseNumberAddition = HouseNumberAddition,
                    PostalCode = PostalCode, City = City, Country = Country,
                    ChamberOfCommerceNumber = ChamberOfCommerceNumber, VATNumber = VATNumber,
                    PaymentTerm = PaymentTerm, DebtorNumber = DebtorNumber, FleetManager = FleetManager,
                    BillingNotes = BillingNotes, Notes = Notes, IsArchived = IsArchived
                });
                var id = SelectedCustomer.Id;
                await RefreshList();
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == id);
            }
            IsEditing = false;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; _logger.Error(ex, "Fout bij opslaan klant"); }
    }

    [RelayCommand]
    private async Task Archive()
    {
        if (SelectedCustomer is null) return;
        try
        {
            await _customerService.UpdateAsync(new UpdateCustomerDto
            {
                Id = SelectedCustomer.Id, CustomerType = CustomerType, FirstName = FirstName, LastName = LastName,
                CompanyName = CompanyName, ContactPerson = ContactPerson, PhoneNumber = PhoneNumber,
                WhatsAppNumber = WhatsAppNumber, Email = Email, IsArchived = true
            });
            await RefreshList();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedCustomer is null) return;
        try { await _customerService.DeleteAsync(SelectedCustomer.Id); SelectedCustomer = null; await RefreshList(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (SelectedCustomer is not null) LoadDetail(SelectedCustomer);
        IsEditing = false; IsNewCustomer = false; ErrorMessage = null;
    }

    // Vehicle dialog
    [RelayCommand]
    private void OpenAddVehicle()
    {
        if (SelectedCustomer is null && !IsNewCustomer) return;
        IsVehicleDialogOpen = true; IsVehicleEditing = false;
        VehiclePlateNumber = string.Empty; VehicleBrand = string.Empty; VehicleModel = string.Empty;
        VehicleYear = DateTime.Now.Year; VehicleChassisNumber = null; VehicleFuelType = FuelType.Benzine;
        VehicleTransmissionType = TransmissionType.Handgeschakeld; VehicleColor = null;
        VehicleMileage = 0; VehicleInspectionExpiryDate = null; VehicleNotes = null; VehicleErrorMessage = null;
    }

    [RelayCommand]
    private void OpenEditVehicle()
    {
        if (SelectedVehicle is null) return;
        IsVehicleDialogOpen = true; IsVehicleEditing = true;
        VehiclePlateNumber = SelectedVehicle.PlateNumberOriginal; VehicleBrand = SelectedVehicle.Brand;
        VehicleModel = SelectedVehicle.Model; VehicleYear = SelectedVehicle.Year;
        VehicleChassisNumber = SelectedVehicle.ChassisNumber; VehicleFuelType = SelectedVehicle.FuelType;
        VehicleTransmissionType = SelectedVehicle.TransmissionType; VehicleColor = SelectedVehicle.Color;
        VehicleMileage = SelectedVehicle.Mileage; VehicleInspectionExpiryDate = SelectedVehicle.InspectionExpiryDate;
        VehicleNotes = SelectedVehicle.Notes; VehicleErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveVehicle()
    {
        try
        {
            VehicleErrorMessage = null;
            var cid = SelectedCustomer?.Id ?? 0;
            if (cid == 0) { VehicleErrorMessage = "Sla eerst de klant op."; return; }

            if (IsVehicleEditing && SelectedVehicle is not null)
                await _vehicleService.UpdateAsync(new UpdateVehicleDto { Id = SelectedVehicle.Id, PlateNumber = VehiclePlateNumber, Brand = VehicleBrand, Model = VehicleModel, Year = VehicleYear, ChassisNumber = VehicleChassisNumber, FuelType = VehicleFuelType, TransmissionType = VehicleTransmissionType, Color = VehicleColor, Mileage = VehicleMileage, InspectionExpiryDate = VehicleInspectionExpiryDate, Notes = VehicleNotes, CustomerId = cid });
            else
                await _vehicleService.CreateAsync(new CreateVehicleDto { PlateNumber = VehiclePlateNumber, Brand = VehicleBrand, Model = VehicleModel, Year = VehicleYear, ChassisNumber = VehicleChassisNumber, FuelType = VehicleFuelType, TransmissionType = VehicleTransmissionType, Color = VehicleColor, Mileage = VehicleMileage, InspectionExpiryDate = VehicleInspectionExpiryDate, Notes = VehicleNotes, CustomerId = cid });

            IsVehicleDialogOpen = false;
            await LoadTabsAsync(cid);
        }
        catch (Exception ex) { VehicleErrorMessage = ex.Message; }
    }

    [RelayCommand] private void CancelVehicle() => IsVehicleDialogOpen = false;

    [RelayCommand]
    private async Task DeleteVehicle()
    {
        if (SelectedVehicle is null || SelectedCustomer is null) return;
        try { await _vehicleService.DeleteAsync(SelectedVehicle.Id); await LoadTabsAsync(SelectedCustomer.Id); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }
}
