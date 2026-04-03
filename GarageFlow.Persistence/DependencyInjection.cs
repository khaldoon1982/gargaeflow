using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Common;
using GarageFlow.Persistence.Context;
using GarageFlow.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<GarageFlowDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<GarageFlowDbContext>());
        services.AddScoped<ISyncQueueRepository, SyncQueueRepository>();

        return services;
    }
}
