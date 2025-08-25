using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Core.Services;
using WeighbridgeService.Infrastructure.Data;
using WeighbridgeService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(WeighbridgeService.Core.Mappings.WeighbridgeProfile));

// Add Entity Framework
builder.Services.AddDbContext<WeighbridgeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=weighbridge.db"));

// Add repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IWeighbridgeRepository, WeighbridgeRepository>();
builder.Services.AddScoped<IWeighbridgeLocationRepository, WeighbridgeLocationRepository>();
builder.Services.AddScoped<IWeighbridgeConfigurationRepository, WeighbridgeConfigurationRepository>();
builder.Services.AddScoped<IWeighbridgeCalibrationRepository, WeighbridgeCalibrationRepository>();
builder.Services.AddScoped<IWeighbridgeMaintenanceRepository, WeighbridgeMaintenanceRepository>();
builder.Services.AddScoped<IWeighbridgeOperatorRepository, WeighbridgeOperatorRepository>();
builder.Services.AddScoped<IWeighbridgeScheduleRepository, WeighbridgeScheduleRepository>();
builder.Services.AddScoped<IWeighbridgeCapacityRepository, WeighbridgeCapacityRepository>();

// Add services
builder.Services.AddScoped<IWeighbridgeService, WeighbridgeService.Core.Services.WeighbridgeService>();

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.RoutePrefix = string.Empty;
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeighbridgeService API V1");
    });
}

// app.UseHttpsRedirection(); // Commented out as requested
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Map health endpoint
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WeighbridgeDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
