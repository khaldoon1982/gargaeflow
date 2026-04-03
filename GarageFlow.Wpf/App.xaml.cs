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
    private static readonly TaskCompletionSource _dbReady = new();

    public static Task WaitForDbAsync() => _dbReady.Task;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env.local");
        if (!File.Exists(envPath))
            envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
        if (File.Exists(envPath))
            DotNetEnv.Env.Load(envPath);

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

        // Show login instantly
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();

        // Init DB in background — login button waits for this
        await Task.Run(async () =>
        {
            try
            {
                using var scope = _host.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                await DatabaseSeeder.SeedAsync(dbContext, hasher.Hash);
                _dbReady.TrySetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database initialisatie mislukt");
                _dbReady.TrySetResult(); // unblock login anyway
            }
        });

        // Start hosted services (sync) after DB is ready
        _ = _host.StartAsync();
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
