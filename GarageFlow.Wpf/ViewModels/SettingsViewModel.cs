using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.Interfaces;
using Microsoft.Win32;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly ILogger _logger;

    [ObservableProperty] private string _workshopName = string.Empty;
    [ObservableProperty] private string _workshopPhone = string.Empty;
    [ObservableProperty] private string _workshopAddress = string.Empty;
    [ObservableProperty] private string _logoPath = string.Empty;
    [ObservableProperty] private string _reminderLeadDays = "7";
    [ObservableProperty] private string _backupFolder = "backups";
    [ObservableProperty] private string? _statusMessage;

    public event Func<Task>? SettingsSaved;

    public SettingsViewModel(ISettingsService settings, ILogger logger) { _settings = settings; _logger = logger; }

    public async Task LoadDataAsync()
    {
        var all = await _settings.GetAllAsync();
        WorkshopName = all.GetValueOrDefault("WorkshopName", "");
        WorkshopPhone = all.GetValueOrDefault("WorkshopPhone", "");
        WorkshopAddress = all.GetValueOrDefault("WorkshopAddress", "");
        LogoPath = all.GetValueOrDefault("LogoPath", "");
        ReminderLeadDays = all.GetValueOrDefault("ReminderLeadDays", "7");
        BackupFolder = all.GetValueOrDefault("BackupFolder", "backups");
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            await _settings.SetAsync("WorkshopName", WorkshopName);
            await _settings.SetAsync("WorkshopPhone", WorkshopPhone);
            await _settings.SetAsync("WorkshopAddress", WorkshopAddress);
            await _settings.SetAsync("LogoPath", LogoPath);
            await _settings.SetAsync("ReminderLeadDays", ReminderLeadDays);
            await _settings.SetAsync("BackupFolder", BackupFolder);
            StatusMessage = "Instellingen opgeslagen!";
            _logger.Information("Instellingen opgeslagen");
            if (SettingsSaved is not null) await SettingsSaved.Invoke();
        }
        catch (Exception ex) { StatusMessage = $"Fout: {ex.Message}"; }
    }

    [RelayCommand]
    private void BrowseLogo()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Afbeeldingen|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Alle bestanden|*.*",
            Title = "Selecteer logo"
        };
        if (dialog.ShowDialog() == true)
        {
            LogoPath = dialog.FileName;
        }
    }
}
