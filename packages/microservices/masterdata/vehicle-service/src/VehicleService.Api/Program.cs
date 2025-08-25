using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Interfaces;
using VehicleService.Core.Mappings;
using VehicleService.Infrastructure.Data;
using VehicleService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(VehicleProfile));

// Add Entity Framework
builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=vehicle.db"));

// Add repositories
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
builder.Services.AddScoped<IVehicleRegistrationRepository, VehicleRegistrationRepository>();
builder.Services.AddScoped<IVehicleSpecificationRepository, VehicleSpecificationRepository>();
builder.Services.AddScoped<IVehicleDocumentRepository, VehicleDocumentRepository>();
builder.Services.AddScoped<IVehicleInspectionRepository, VehicleInspectionRepository>();
builder.Services.AddScoped<IVehicleInsuranceRepository, VehicleInsuranceRepository>();

// Add services
builder.Services.AddScoped<IVehicleService, VehicleService.Core.Services.VehicleService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicle Service API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
