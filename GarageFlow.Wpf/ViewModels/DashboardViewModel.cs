using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;

namespace GarageFlow.Wpf.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private readonly IGlobalSearchService _searchService;

    // Counts
    [ObservableProperty] private int _totalCustomers;
    [ObservableProperty] private int _totalVehicles;
    [ObservableProperty] private int _maintenanceThisMonth;
    [ObservableProperty] private int _inspectionsDueIn7Days;
    [ObservableProperty] private int _expiredInspections;
    [ObservableProperty] private int _openReminders;
    [ObservableProperty] private int _activeMaintenanceJobs;

    // Financials
    [ObservableProperty] private decimal _revenueThisMonth;
    [ObservableProperty] private decimal _revenueLastMonth;
    [ObservableProperty] private decimal _totalOutstandingCosts;
    [ObservableProperty] private decimal _averageJobCost;

    // Lists
    [ObservableProperty] private ObservableCollection<MaintenanceRecordDto> _recentMaintenance = new();
    [ObservableProperty] private ObservableCollection<InspectionDto> _upcomingInspections = new();
    [ObservableProperty] private ObservableCollection<MaintenanceRecordDto> _activeJobs = new();
    [ObservableProperty] private ObservableCollection<VehicleAlertDto> _vehicleAlerts = new();

    // Search
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private ObservableCollection<SearchResultItem> _searchResults = new();
    [ObservableProperty] private bool _isSearching;
    [ObservableProperty] private SearchResultItem? _selectedSearchResult;

    /// <summary>Fired when user clicks a search result. Tuple: (type, id) e.g. ("Klant", 3)</summary>
    public event Action<string, int>? NavigateToResult;

    public DashboardViewModel(IDashboardService dashboardService, IGlobalSearchService searchService)
    {
        _dashboardService = dashboardService;
        _searchService = searchService;
    }

    public async Task LoadDataAsync()
    {
        var data = await _dashboardService.GetDashboardAsync();
        TotalCustomers = data.TotalCustomers;
        TotalVehicles = data.TotalVehicles;
        MaintenanceThisMonth = data.MaintenanceThisMonth;
        InspectionsDueIn7Days = data.InspectionsDueIn7Days;
        ExpiredInspections = data.ExpiredInspections;
        OpenReminders = data.OpenReminders;
        ActiveMaintenanceJobs = data.ActiveMaintenanceJobs;
        RevenueThisMonth = data.RevenueThisMonth;
        RevenueLastMonth = data.RevenueLastMonth;
        TotalOutstandingCosts = data.TotalOutstandingCosts;
        AverageJobCost = data.AverageJobCost;
        RecentMaintenance = new ObservableCollection<MaintenanceRecordDto>(data.RecentMaintenance);
        UpcomingInspections = new ObservableCollection<InspectionDto>(data.UpcomingInspections);
        ActiveJobs = new ObservableCollection<MaintenanceRecordDto>(data.ActiveJobs);
        VehicleAlerts = new ObservableCollection<VehicleAlertDto>(data.VehicleAlerts);
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2)
        {
            SearchResults = new();
            IsSearching = false;
            return;
        }
        IsSearching = true;
        var result = await _searchService.SearchAsync(SearchQuery);
        SearchResults = new ObservableCollection<SearchResultItem>(result.Results);
    }

    // Auto-search on every keystroke after 2 chars
    async partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        {
            SearchResults = new();
            IsSearching = false;
            return;
        }
        await Search();
    }

    partial void OnSelectedSearchResultChanged(SearchResultItem? value)
    {
        if (value is null) return;
        NavigateToResult?.Invoke(value.Type, value.Id);
        SearchQuery = string.Empty;
        SearchResults = new();
        IsSearching = false;
        SelectedSearchResult = null;
    }
}
