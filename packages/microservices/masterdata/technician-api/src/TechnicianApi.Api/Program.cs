using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using FluentValidation;
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

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<TechnicianApiDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Host=db;Port=5432;Database=techniciandb;Username=technician;Password=technician123;Pooling=true;MinPoolSize=5;MaxPoolSize=100",
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));

// Configure static files
builder.Services.AddDirectoryBrowser();

// Add AutoMapper with profiles
builder.Services.AddAutoMapper(
    typeof(Program).Assembly,
    typeof(FinancialFormsProfile).Assembly,
    typeof(TechnicianProfile).Assembly,
    typeof(AssignmentProfile).Assembly,
    typeof(CheckInProfile).Assembly,
    typeof(PhotoProfile).Assembly,
    typeof(ServiceReportProfile).Assembly,
    typeof(RequisitionProfile).Assembly,
    typeof(DailySummaryProfile).Assembly,
    typeof(PerformanceMetricsProfile).Assembly,
    typeof(AttachmentProfile).Assembly);

// Configure Entity Framework - Conditional based on environment
if (builder.Environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
{
    // Use In-Memory for tests
    builder.Services.AddDbContext<TechnicianApiDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString("N")[..8]));
}
else
{
    // Use SQLite for development/production
    builder.Services.AddDbContext<TechnicianApiDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Data Source=technician-api.db"));
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
        context.Database.Migrate();
        
        // Uncomment to seed initial data if needed
        // var dbInitializer = services.GetRequiredService<DbInitializer>();
        // await dbInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
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

// CORS must be after UseRouting() but before UseAuthentication() and UseAuthorization()
app.UseCors("AllowAll");

app.MapControllers();
app.MapHealthChecks("/health");

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