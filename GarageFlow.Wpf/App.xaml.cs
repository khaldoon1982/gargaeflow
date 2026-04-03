using System.IO;
using System.Windows;
using GarageFlow.Application;
using GarageFlow.Application.Interfaces;
using GarageFlow.Infrastructure;
using GarageFlow.Persistence;
using GarageFlow.Persistence.Context;
using GarageFlow.Persistence.Seed;
using GarageFlow.Wpf.ViewModels;
using GarageFlow.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GarageFlow.Wpf;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "garageflow.db");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Load env FAST — no heavy I/O
        var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env.local");
        if (!File.Exists(envPath))
            envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
        if (File.Exists(envPath))
            DotNetEnv.Env.Load(envPath);

        // Minimal host — no appsettings, no env provider, no user secrets
        _host = new HostBuilder()
            .UseSerilog((_, config) =>
            {
                config.MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("logs/garageflow-.log", rollingInterval: RollingInterval.Day);
            })
            .ConfigureServices(services =>
            {
                services.AddApplication();
                services.AddPersistence($"Data Source={DbPath}");
                services.AddInfrastructure(DbPath);

                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CustomerViewModel>();
                services.AddTransient<VehicleViewModel>();
                services.AddTransient<MaintenanceViewModel>();
                services.AddTransient<InspectionViewModel>();
                services.AddTransient<ReminderViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<BackupViewModel>();

                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        // Show login IMMEDIATELY
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();

        // Start host + DB init in background — doesn't block the UI
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _host!.StartAsync();

            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DatabaseSeeder.SeedAsync(dbContext, hasher.Hash);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Initialisatie mislukt");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }
        base.OnExit(e);
    }

    public static IHost GetHost() => ((App)Current)._host!;
}
