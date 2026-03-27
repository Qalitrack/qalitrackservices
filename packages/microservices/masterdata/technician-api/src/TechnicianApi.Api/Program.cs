using Microsoft.EntityFrameworkCore;
using Prometheus;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.Services;
using TechnicianApi.Core.DTOs;
using TechnicianApi.Core.Mappings;
using TechnicianApi.Core.BackgroundServices;
using TechnicianApi.Infrastructure.Data;
using TechnicianApi.Infrastructure.Repositories;
using TechnicianApi.Api.Middleware;
using TechnicianApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/technician-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HttpContextAccessor for accessing the current HTTP context in services
builder.Services.AddHttpContextAccessor();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "TechnicianApi API", 
        Version = "v1",
        Description = @"QaliTrack TechnicianApi - Technician API service"
    });
    

});

// Add AutoMapper

// Add file storage service
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Configure static files
builder.Services.AddDirectoryBrowser();

// Add AutoMapper with profiles (AutoMapper 13+ API)
builder.Services.AddAutoMapper(cfg => { },
    typeof(Program),
    typeof(FinancialFormsProfile),
    typeof(TechnicianProfile),
    typeof(AssignmentProfile),
    typeof(CheckInProfile),
    typeof(PhotoProfile),
    typeof(ServiceReportProfile),
    typeof(RequisitionProfile),
    typeof(DailySummaryProfile),
    typeof(PerformanceMetricsProfile),
    typeof(AttachmentProfile),
    typeof(DriverMappingProfile),
    typeof(FleetProfile),
    typeof(TripProfile),
    typeof(FeedbackProfile),
    typeof(SystemSettingsProfile));

// Configure Entity Framework based on environment
if (builder.Environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
{
    // Use In-Memory for tests
    builder.Services.AddDbContext<TechnicianApiDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString("N")[..8])
              .ConfigureWarnings(warnings => 
                  warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
}
else if (builder.Configuration.GetValue<bool>("UsePostgreSQL") == true)
{
    // Use PostgreSQL if explicitly configured
    builder.Services.AddDbContext<TechnicianApiDbContext>((serviceProvider, options) =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection") ?? 
            "Host=db;Port=5435;Database=techniciandb;Username=technician;Password=technician123;Pooling=true;MinPoolSize=5;MaxPoolSize=100",
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
            
        // Suppress the pending model changes warning
        options.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}
else
{
    // Default to SQLite for development/production
    builder.Services.AddDbContext<TechnicianApiDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
            "Data Source=technician-api.db");
            
        // Suppress the pending model changes warning
        options.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}

// Add generic repository (shared by all entities)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add specialized repositories
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();

// Add services
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IServiceReportService, ServiceReportService>();
builder.Services.AddScoped<IRequisitionService, RequisitionService>();
builder.Services.AddScoped<IDailySummaryService, DailySummaryService>();
builder.Services.AddScoped<IPerformanceMetricsService, PerformanceMetricsService>();

// Add financial forms services
builder.Services.AddScoped<IPettyCashAdvanceFormService, PettyCashAdvanceFormService>();
builder.Services.AddScoped<IAdvanceReturnFormService, AdvanceReturnFormService>();
builder.Services.AddScoped<IPerDiemReturnFormService, PerDiemReturnFormService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAssignmentBalanceService, AssignmentBalanceService>();

// Add QTruck services (merged from QTruck API)
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IDriverProfileService, DriverProfileService>();
builder.Services.AddScoped<IDriverActivityService, DriverActivityService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IMaterialVariantService, MaterialVariantService>();
builder.Services.AddScoped<IMaterialCostService, MaterialCostService>();
builder.Services.AddScoped<IMaterialPhotoService, MaterialPhotoService>();
builder.Services.AddScoped<IMaterialVariantPhotoService, MaterialVariantPhotoService>();
builder.Services.AddScoped<ITruckService, TruckService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<ITripTypeService, TripTypeService>();
builder.Services.AddScoped<ITripMaterialService, TripMaterialService>();
builder.Services.AddScoped<IVehicleMileageService, VehicleMileageService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<ILicenseClassService, LicenseClassService>();

// Add CORS with more permissive settings for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true) // Allow any origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Important for cookies, authorization headers with HTTPS
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Background Services for automated daily summaries and performance metrics
builder.Services.AddHostedService<DailySummaryBackgroundService>();
builder.Services.AddHostedService<PerformanceMetricsBackgroundService>();

var app = builder.Build();

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TechnicianApiDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Check if there are any pending migrations
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying the following migrations: {Migrations}", string.Join(", ", pendingMigrations));
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations to apply.");
            
            // Check if the database exists and has the expected tables
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                logger.LogInformation("Database connection test: {Status}", canConnect ? "Success" : "Failed");
            }
            catch (Exception dbEx)
            {
                logger.LogError(dbEx, "Error connecting to the database. The database might need to be created.");
                throw;
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw; // Re-throw to prevent app from starting with a broken database
    }
}

// Enable CORS before other middleware
app.UseCors("AllowAll");

// Security middleware
app.UseMiddleware<RequestLoggingSanitizerMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<SensitiveDataFilterMiddleware>();

// Enable static files and set up the uploads directory
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Configure the HTTP request pipeline
// ALWAYS generate Swagger JSON (for both DEV + PROD)
app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechnicianApi API V1");
        c.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "QaliTrack Services - TechnicianApi & Gateway Discovery";
    });
}
else
{
    // ✅ PROD: Read-Only (blocks POST/PUT/DELETE via Traefik JWT)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "TechnicianApi API V1");
        c.RoutePrefix = "swagger";  // ← FIXED: Serve UI at /swagger
        c.DocumentTitle = "QaliTrack TechnicianApi API (Read-Only)";
    });
}

app.UseHttpsRedirection();

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseRouting();
app.UseHttpMetrics();

// CORS must be after UseRouting() but before UseAuthentication() and UseAuthorization()
app.UseCors("AllowAll");

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

// Initialize database with proper error handling (skip in test environment)
if (!app.Environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<TechnicianApiDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync(); // Use async version
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw; // Re-throw to prevent app from starting with a broken database
        }
    }
}

Log.Information("TechnicianApi starting up...");
app.Run();

// Make Program class accessible for testing
public partial class Program { }