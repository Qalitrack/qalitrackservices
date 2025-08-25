using Microsoft.EntityFrameworkCore;
using DriverService.Core.Interfaces;
using DriverService.Core.Mappings;
using DriverService.Infrastructure.Data;
using DriverService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(DriverProfile));

// Add Entity Framework
builder.Services.AddDbContext<DriverDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=driver.db"));

// Add repositories
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverLicenseRepository, DriverLicenseRepository>();
builder.Services.AddScoped<IDriverProfileRepository, DriverProfileRepository>();
builder.Services.AddScoped<IDriverDocumentRepository, DriverDocumentRepository>();
builder.Services.AddScoped<IDriverViolationRepository, DriverViolationRepository>();

// Add services
builder.Services.AddScoped<IDriverService, DriverService.Core.Services.DriverService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DriverDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();