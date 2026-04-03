using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.Interfaces;
using Microsoft.Win32;
using Serilog;

namespace GarageFlow.Wpf.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly IBackupService _backupService;
    private readonly ISettingsService _settings;
    private readonly ILogger _logger;

    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private bool _isBusy;

    public BackupViewModel(IBackupService backupService, ISettingsService settings, ILogger logger) { _backupService = backupService; _settings = settings; _logger = logger; }

    [RelayCommand]
    private async Task CreateBackup()
    {
        try
        {
            IsBusy = true;
            var folder = await _settings.GetAsync("BackupFolder");
            var path = await _backupService.CreateBackupAsync(folder);
            StatusMessage = $"Back-up succesvol aangemaakt: {path}";
        }
        catch (Exception ex) { StatusMessage = $"Fout bij back-up: {ex.Message}"; _logger.Error(ex, "Back-up mislukt"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RestoreBackup()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Database bestanden|*.db|Alle bestanden|*.*",
            Title = "Selecteer back-up bestand"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            IsBusy = true;
            await _backupService.RestoreBackupAsync(dialog.FileName);
            StatusMessage = "Back-up succesvol hersteld! Herstart de applicatie.";
        }
        catch (Exception ex) { StatusMessage = $"Fout bij herstellen: {ex.Message}"; _logger.Error(ex, "Herstel mislukt"); }
        finally { IsBusy = false; }
    }
}
