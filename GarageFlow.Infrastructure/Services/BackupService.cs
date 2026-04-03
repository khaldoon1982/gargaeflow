using GarageFlow.Application.Interfaces;
using Serilog;

namespace GarageFlow.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly string _dbPath;
    private readonly ILogger _logger;

    public BackupService(string dbPath, ILogger logger)
    {
        _dbPath = dbPath;
        _logger = logger;
    }

    public async Task<string> CreateBackupAsync(string? destinationFolder = null)
    {
        destinationFolder ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        Directory.CreateDirectory(destinationFolder);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(destinationFolder, $"garageflow_backup_{timestamp}.db");

        await Task.Run(() => File.Copy(_dbPath, backupPath, overwrite: true));
        _logger.Information("Back-up aangemaakt: {Path}", backupPath);
        return backupPath;
    }

    public async Task RestoreBackupAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Back-up bestand niet gevonden.", backupFilePath);

        await Task.Run(() => File.Copy(backupFilePath, _dbPath, overwrite: true));
        _logger.Information("Back-up hersteld van: {Path}", backupFilePath);
    }
}
