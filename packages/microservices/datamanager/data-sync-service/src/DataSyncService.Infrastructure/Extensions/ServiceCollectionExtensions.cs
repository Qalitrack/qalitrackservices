using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DataSyncService.Core.Interfaces;
using DataSyncService.Infrastructure.Data;
using DataSyncService.Infrastructure.Repositories;
using DataSyncService.Infrastructure.Services;

namespace DataSyncService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<DataSyncDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);
        });

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISyncSessionRepository, SyncSessionRepository>();
        services.AddScoped<ISyncSiteRepository, SyncSiteRepository>();
        services.AddScoped<IChangeRecordRepository, ChangeRecordRepository>();
        services.AddScoped<ISyncConflictRepository, SyncConflictRepository>();

        // Add Infrastructure Services
        services.AddScoped<ISyncEngine, SyncEngine>();
        services.AddScoped<ISiteHealthMonitor, SiteHealthMonitor>();
        services.AddScoped<IConflictDetectionEngine, ConflictDetectionEngine>();
        services.AddScoped<IConflictResolver, ConflictResolver>();

        return services;
    }
}