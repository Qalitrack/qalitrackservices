using FluentValidation;
using FluentValidation.AspNetCore;
using WeightDataService.Core.Interfaces;
using WeightDataService.Core.Services;
using WeightDataService.Core.Validators;

namespace WeightDataService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Business services
        services.AddScoped<IWeightMeasurementService, WeightMeasurementService>();
        services.AddScoped<IWeighbridgeStatusService, WeighbridgeStatusService>();
        services.AddScoped<IWeightCorrectionService, WeightCorrectionService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // AutoMapper
        services.AddAutoMapper(typeof(WeightDataService.Core.Mappings.WeightMeasurementProfile).Assembly);

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateWeightMeasurementValidator>();

        return services;
    }
}