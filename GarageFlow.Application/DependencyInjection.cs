using FluentValidation;
using GarageFlow.Application.Interfaces;
using GarageFlow.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IMaintenanceRecordService, MaintenanceRecordService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISettingsService, SettingsService>();

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
