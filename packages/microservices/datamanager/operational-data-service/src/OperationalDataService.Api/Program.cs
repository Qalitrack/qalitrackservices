using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using OperationalDataService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/operational-data-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
}).AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Operational Data Service API", 
        Version = "v1",
        Description = "QaliTrack Operational Data Service - Manages operational data, performance metrics, and business intelligence"
    });
    
    // Add versioning support for Swagger
    c.DocInclusionPredicate((version, desc) => true);
});

// Add Entity Framework
builder.Services.AddDbContext<OperationalDataDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=operationaldata.db"));

// Register Repositories and UnitOfWork
builder.Services.AddScoped(typeof(OperationalDataService.Core.Interfaces.IRepository<>), typeof(OperationalDataService.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.IUnitOfWork, OperationalDataService.Infrastructure.Repositories.UnitOfWork>();

// Register Services  
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.IOperationalDataService, OperationalDataService.Core.Services.OperationalDataService>();
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.ICapacityManagementService, OperationalDataService.Core.Services.CapacityManagementService>();
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.IOperationalDashboardService, OperationalDataService.Core.Services.OperationalDashboardService>();
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.IRouteOptimizationService, OperationalDataService.Core.Services.RouteOptimizationService>();
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.IProductCatalogService, OperationalDataService.Core.Services.ProductCatalogService>();
builder.Services.AddScoped<OperationalDataService.Core.Interfaces.IMasterDataIntegrationService, OperationalDataService.Infrastructure.ExternalServices.MasterDataIntegrationService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(OperationalDataService.Core.Mappings.OperationalDataMappingProfile));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
// Always enable Swagger for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Operational Data Service API V1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

Log.Information("Swagger configured - Environment: {Environment}", app.Environment.EnvironmentName);

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OperationalDataDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Log.Information("Database ensured created successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error creating database");
        throw;
    }
}

Log.Information("Operational Data Service starting up...");
app.Run();