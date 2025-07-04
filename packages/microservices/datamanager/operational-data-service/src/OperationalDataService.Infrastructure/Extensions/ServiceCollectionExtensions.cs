using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OperationalDataService.Core.Interfaces;
using OperationalDataService.Core.Mappings;
using OperationalDataService.Core.Services;
using OperationalDataService.Infrastructure.Data;
using OperationalDataService.Infrastructure.ExternalServices;
using OperationalDataService.Infrastructure.Repositories;

namespace OperationalDataService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<OperationalDataDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                                 "Data Source=operational_data_service.db";
            options.UseSqlite(connectionString);
        });

        // Add AutoMapper
        services.AddAutoMapper(typeof(OperationalDataMappingProfile));

        // Add Unit of Work and Repository
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Core Services
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();
        services.AddScoped<ICapacityManagementService, CapacityManagementService>();
        services.AddScoped<IOperationalDashboardService, OperationalDashboardService>();

        // Add External Services
        services.AddHttpClient<IMasterDataIntegrationService, MasterDataIntegrationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "OperationalDataService/1.0");
        });

        services.AddScoped<IMasterDataIntegrationService, MasterDataIntegrationService>();

        // Add Memory Cache for caching
        services.AddMemoryCache();

        // Add Redis if configured
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }

        return services;
    }

    public static async Task<IServiceProvider> MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OperationalDataDbContext>();
        
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // Log migration error
            Console.WriteLine($"Migration failed: {ex.Message}");
            throw;
        }

        return serviceProvider;
    }
}