using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private ObservableObject? _currentViewModel;
    [ObservableProperty] private UserDto? _currentUser;
    [ObservableProperty] private string _title = "GarageFlow - Garagebeheer";
    [ObservableProperty] private string? _logoPath;
    [ObservableProperty] private string _workshopName = "GarageFlow";

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task NavigateToDashboard()
    {
        var vm = _serviceProvider.GetRequiredService<DashboardViewModel>();
        vm.NavigateToResult += OnSearchResultSelected;
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    private async void OnSearchResultSelected(string type, int id)
    {
        if (type == "Klant")
        {
            var vm = _serviceProvider.GetRequiredService<CustomerViewModel>();
            CurrentViewModel = vm;
            await vm.LoadDataAsync();
            vm.SelectedCustomer = vm.Customers.FirstOrDefault(c => c.Id == id);
        }
        else if (type == "Voertuig")
        {
            var vm = _serviceProvider.GetRequiredService<CustomerViewModel>();
            CurrentViewModel = vm;
            await vm.LoadDataAsync();
            // Find which customer owns this vehicle and navigate there
            var vehicleService = _serviceProvider.GetRequiredService<Application.Interfaces.IVehicleService>();
            var vehicle = await vehicleService.GetByIdAsync(id);
            if (vehicle is not null)
            {
                vm.SelectedCustomer = vm.Customers.FirstOrDefault(c => c.Id == vehicle.CustomerId);
                // Switch to vehicles tab after loading
            }
        }
    }

    [RelayCommand]
    private async Task NavigateToCustomers()
    {
        var vm = _serviceProvider.GetRequiredService<CustomerViewModel>();
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToVehicles()
    {
        var vm = _serviceProvider.GetRequiredService<VehicleViewModel>();
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToMaintenance()
    {
        var vm = _serviceProvider.GetRequiredService<MaintenanceViewModel>();
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToInspections()
    {
        var vm = _serviceProvider.GetRequiredService<InspectionViewModel>();
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToReminders()
    {
        var vm = _serviceProvider.GetRequiredService<ReminderViewModel>();
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToSettings()
    {
        var vm = _serviceProvider.GetRequiredService<SettingsViewModel>();
        vm.SettingsSaved += RefreshSettingsAsync;
        CurrentViewModel = vm;
        await vm.LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToBackup()
    {
        var vm = _serviceProvider.GetRequiredService<BackupViewModel>();
        CurrentViewModel = vm;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LoadInitialData()
    {
        await RefreshSettingsAsync();
        await NavigateToDashboard();
    }

    public async Task RefreshSettingsAsync()
    {
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        LogoPath = await settingsService.GetAsync("LogoPath");
        var name = await settingsService.GetAsync("WorkshopName");
        WorkshopName = string.IsNullOrWhiteSpace(name) ? "GarageFlow" : name;
        Title = $"{WorkshopName} - Garagebeheer";
    }

    [RelayCommand]
    private void Logout()
    {
        CurrentUser = null;
        var loginWindow = _serviceProvider.GetRequiredService<Views.LoginWindow>();
        loginWindow.Show();
        System.Windows.Application.Current.MainWindow?.Close();
    }
}
