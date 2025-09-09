using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Infrastructure.Data;
using QaliTrack.DataManager.Api.Services;
using System.Text.Json.Serialization;
using AutoMapper;

// Load environment variables from .env file
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
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
    Console.WriteLine($"Loading environment variables from: {envFile}");
}

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add database configuration service
builder.Services.AddScoped<IDatabaseConfigurationService, DatabaseConfigurationService>();

// Database context with multi-provider support
builder.Services.AddDbContext<DataManagerDbContext>((serviceProvider, options) =>
{
    var dbConfigService = serviceProvider.GetRequiredService<IDatabaseConfigurationService>();
    var connectionString = dbConfigService.GetConnectionStringAsync().Result;
    var provider = dbConfigService.GetDatabaseProvider();
    
    switch (provider.ToUpper())
    {
        case "SQLITE":
            options.UseSqlite(connectionString);
            break;
        case "POSTGRESQL":
        case "POSTGRES":
            options.UseNpgsql(connectionString);
            break;
        default:
            throw new NotSupportedException($"Database provider '{provider}' is not supported.");
    }
    
    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

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
    .AddDbContextCheck<DataManagerDbContext>();

var app = builder.Build();

// Auto-migrate database if enabled
var autoMigrate = bool.Parse(Environment.GetEnvironmentVariable("AUTO_MIGRATE") ?? "false");
if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<DataManagerDbContext>();
    
    try
    {
        logger.LogInformation("Running database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments except Production
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QaliTrack DataManager API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

// Use CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Add health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var dbConfigService = context.RequestServices.GetRequiredService<IDatabaseConfigurationService>();
        var provider = dbConfigService.GetDatabaseProvider();
        
        var response = new
        {
            status = report.Status.ToString(),
            provider = provider,
            environment = app.Environment.EnvironmentName
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.Run();