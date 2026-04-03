using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;

namespace GarageFlow.Wpf.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;

    [ObservableProperty] private int _totalCustomers;
    [ObservableProperty] private int _totalVehicles;
    [ObservableProperty] private int _maintenanceThisMonth;
    [ObservableProperty] private int _inspectionsDueIn7Days;
    [ObservableProperty] private int _expiredInspections;
    [ObservableProperty] private int _openReminders;
    [ObservableProperty] private ObservableCollection<MaintenanceRecordDto> _recentMaintenance = new();
    [ObservableProperty] private ObservableCollection<InspectionDto> _upcomingInspections = new();

    public DashboardViewModel(IDashboardService dashboardService) { _dashboardService = dashboardService; }

    public async Task LoadDataAsync()
    {
        var data = await _dashboardService.GetDashboardAsync();
        TotalCustomers = data.TotalCustomers;
        TotalVehicles = data.TotalVehicles;
        MaintenanceThisMonth = data.MaintenanceThisMonth;
        InspectionsDueIn7Days = data.InspectionsDueIn7Days;
        ExpiredInspections = data.ExpiredInspections;
        OpenReminders = data.OpenReminders;
        RecentMaintenance = new ObservableCollection<MaintenanceRecordDto>(data.RecentMaintenance);
        UpcomingInspections = new ObservableCollection<InspectionDto>(data.UpcomingInspections);
    }
}
