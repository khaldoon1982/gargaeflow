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
    private readonly IHost _host;
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "garageflow.db");

    public App()
    {
        var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env.local");
        if (!File.Exists(envPath))
            envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
        if (File.Exists(envPath))
            DotNetEnv.Env.Load(envPath);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog((context, config) =>
            {
                config
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("logs/garageflow-.log", rollingInterval: RollingInterval.Day);
            })
            .ConfigureServices((context, services) =>
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
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Show login window IMMEDIATELY — don't block on host/db
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();

        base.OnStartup(e);

        // Heavy work in background — user sees the login screen instantly
        await Task.Run(async () =>
        {
            await _host.StartAsync();

            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DatabaseSeeder.SeedAsync(dbContext, hasher.Hash);
        });
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }

    public static IHost GetHost() => ((App)Current)._host;
}
