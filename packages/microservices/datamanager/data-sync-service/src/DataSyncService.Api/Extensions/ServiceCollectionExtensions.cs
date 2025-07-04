using FluentValidation;
using FluentValidation.AspNetCore;
using DataSyncService.Core.Interfaces;
using DataSyncService.Core.Mappings;
using DataSyncService.Core.Services;
using DataSyncService.Core.Validators;
using DataSyncService.Infrastructure.Extensions;

namespace DataSyncService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Infrastructure services
        services.AddInfrastructure(configuration);

        // Add Core services
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<ISyncSiteService, SyncSiteService>();
        services.AddScoped<IConflictResolutionService, ConflictResolutionService>();

        // Add AutoMapper
        services.AddAutoMapper(typeof(DataSyncMappingProfile));

        // Add FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<CreateSyncSessionValidator>();

        // Add HttpClient for health monitoring
        services.AddHttpClient();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // Add API versioning - simplified for now
        services.AddApiVersioning();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Data Sync Service API",
                Version = "v1",
                Description = "API for managing multi-site data synchronization, conflict resolution, and site health monitoring",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@example.com"
                }
            });

            // Include XML comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add enum descriptions
            c.SchemaFilter<EnumSchemaFilter>();

            // Configure security if needed
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
        });

        return services;
    }
}

public class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            Enum.GetNames(context.Type)
                .ToList()
                .ForEach(name => schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name)));
        }
    }
}