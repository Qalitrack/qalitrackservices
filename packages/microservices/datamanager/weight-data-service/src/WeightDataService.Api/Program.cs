using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using WeightDataService.Core.Interfaces;
using WeightDataService.Core.Services;
using WeightDataService.Core.Validators;
using WeightDataService.Infrastructure.Data;
using WeightDataService.Infrastructure.Repositories;
using WeightDataService.Infrastructure.Extensions;
using WeightDataService.Api.Extensions;
using WeightDataService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Weight Data Service API",
        Version = "v1",
        Description = "API for managing weight measurements and weighbridge data"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database configuration
builder.Services.AddDbContext<WeightDataContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=weightdata.db";
    options.UseSqlite(connectionString);
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(WeightDataService.Core.Mappings.WeightMeasurementProfile).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateWeightMeasurementValidator>();

// Repository pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWeightMeasurementRepository, WeightMeasurementRepository>();
builder.Services.AddScoped<IWeighbridgeStatusRepository, WeighbridgeStatusRepository>();
builder.Services.AddScoped<IWeightCorrectionRepository, WeightCorrectionRepository>();

// Business services
builder.Services.AddScoped<IWeightMeasurementService, WeightMeasurementService>();
builder.Services.AddScoped<IWeighbridgeStatusService, WeighbridgeStatusService>();
builder.Services.AddScoped<IWeightCorrectionService, WeightCorrectionService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WeightDataContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weight Data Service API v1");
        c.RoutePrefix = string.Empty; // Make Swagger the default page
    });
}

app.UseRouting();
app.UseCors("AllowAll");

// Custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WeightDataContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();