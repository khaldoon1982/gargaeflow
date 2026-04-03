namespace GarageFlow.Application.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string? destinationFolder = null);
    Task RestoreBackupAsync(string backupFilePath);
}
