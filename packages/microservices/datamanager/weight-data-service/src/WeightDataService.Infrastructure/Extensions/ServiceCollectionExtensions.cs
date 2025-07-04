using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;
using WeightDataService.Infrastructure.Repositories;

namespace WeightDataService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<WeightDataContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWeightMeasurementRepository, WeightMeasurementRepository>();
        services.AddScoped<IWeighbridgeStatusRepository, WeighbridgeStatusRepository>();
        services.AddScoped<IWeightCorrectionRepository, WeightCorrectionRepository>();

        return services;
    }
}