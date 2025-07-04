using Microsoft.OpenApi.Models;
using System.Reflection;

namespace OperationalDataService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Add Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Operational Data Service API",
                Version = "v1",
                Description = "A comprehensive operational data management system for weighbridge operations with real-time synchronization, optimization algorithms, and advanced operational capabilities",
                Contact = new OpenApiContact
                {
                    Name = "QaliTrack Systems",
                    Email = "support@qalitrack.com"
                }
            });

            // Include XML comments for better documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add security definition for JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Configure enums to use string values
            c.UseInlineDefinitionsForEnums();
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add response caching
        services.AddResponseCaching();

        // Add memory cache
        services.AddMemoryCache();

        // Add API versioning (commented out for now)
        // services.AddApiVersioning(options =>
        // {
        //     options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        //     options.AssumeDefaultVersionWhenUnspecified = true;
        //     options.ApiVersionReader = Microsoft.AspNetCore.Mvc.ApiVersionReader.Combine(
        //         new Microsoft.AspNetCore.Mvc.QueryStringApiVersionReader("version"),
        //         new Microsoft.AspNetCore.Mvc.HeaderApiVersionReader("X-Version"),
        //         new Microsoft.AspNetCore.Mvc.MediaTypeApiVersionReader("ver")
        //     );
        // });

        // services.AddVersionedApiExplorer(setup =>
        // {
        //     setup.GroupNameFormat = "'v'VVV";
        //     setup.SubstituteApiVersionInUrl = true;
        // });

        return services;
    }
}