using GarageFlow.Application.Interfaces;
using GarageFlow.Infrastructure.Services;
using GarageFlow.Infrastructure.Sync;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GarageFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbPath)
    {
        services.AddSingleton<IPlateNormalizationService, PlateNormalizationService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IBackupService>(sp => new BackupService(dbPath, sp.GetRequiredService<ILogger>()));

        // Sync
        var syncConfig = SyncConfiguration.FromEnvironment();
        services.AddSingleton(syncConfig);
        services.AddHttpClient<ISyncApiClient, SyncApiClient>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddHostedService<BackgroundSyncService>();

        // Email (Resend)
        services.AddHttpClient<IEmailService, ResendEmailService>();

        // Google Calendar
        services.AddSingleton<ICalendarService, GoogleCalendarService>();

        return services;
    }
}
