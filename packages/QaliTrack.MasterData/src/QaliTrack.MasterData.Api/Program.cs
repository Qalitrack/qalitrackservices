using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Infrastructure.Data;
using QaliTrack.MasterData.Infrastructure.Repositories;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Api.Services;
using AutoMapper;
using System.Text.Json.Serialization;
using SqliteExceptions = EntityFramework.Exceptions.Sqlite.ExceptionProcessorExtensions;
using PostgresExceptions = EntityFramework.Exceptions.PostgreSQL.ExceptionProcessorExtensions;

var builder = WebApplication.CreateBuilder(args);

// Load .env file if it exists
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    Console.WriteLine($"Loading environment variables from: {envFile}");
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            
            // Only set from .env if environment variable doesn't already exist
            // This ensures Docker environment variables take precedence
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "QaliTrack MasterData API", 
        Version = "v1",
        Description = "Master data management for QaliTrack weighbridge system - Cement industry operations"
    });
});

// Database configuration service
builder.Services.AddScoped<IDatabaseConfigurationService, DatabaseConfigurationService>();

// Database context with flexible provider
builder.Services.AddDbContext<MasterDataDbContext>((serviceProvider, options) =>
{
    var dbConfigService = serviceProvider.GetRequiredService<IDatabaseConfigurationService>();
    var connectionString = dbConfigService.GetConnectionStringAsync().Result;
    var provider = dbConfigService.GetDatabaseProvider();

    switch (provider.ToUpper())
    {
        case "SQLITE":
            options.UseSqlite(connectionString);
            SqliteExceptions.UseExceptionProcessor(options);
            break;
        case "POSTGRESQL":
        case "POSTGRES":
            options.UseNpgsql(connectionString);
            PostgresExceptions.UseExceptionProcessor(options);
            break;
        default:
            throw new NotSupportedException($"Database provider '{provider}' is not supported.");
    }

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine);
    }
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Repository pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MasterDataDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments except Production
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QaliTrack MasterData API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

// Use CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Simple health endpoint with service info
app.MapGet("/health/info", (IDatabaseConfigurationService dbConfig) => new { 
    status = "healthy", 
    service = "QaliTrack MasterData API",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    database_provider = dbConfig.GetDatabaseProvider(),
    environment = app.Environment.EnvironmentName
});

// Auto-migrate database on startup (optional, can be disabled via environment variable)
var autoMigrate = Environment.GetEnvironmentVariable("AUTO_MIGRATE")?.ToLower() != "false";
if (autoMigrate)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MasterDataDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Running database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error running database migrations");
        
        // Don't fail startup if migrations fail, just log the error
        // This allows the service to start and be diagnosed
    }
}

app.Run();