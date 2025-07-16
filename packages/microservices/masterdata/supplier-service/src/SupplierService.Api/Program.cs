using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using SupplierService.Core.Interfaces;
using SupplierService.Core.Mappings;
using SupplierService.Core.Entities;
using SupplierService.Core.Services;
using SupplierService.Infrastructure.Data;
using SupplierService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database Configuration
builder.Services.AddDbContext<SupplierDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=supplier.db"));

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(SupplierProfile));

// FluentValidation Configuration (commented out until validators are implemented)
// builder.Services.AddFluentValidationAutoValidation();
// builder.Services.AddValidatorsFromAssemblyContaining<RegisterSupplierValidator>();

// Repository Registration
builder.Services.AddScoped<IRepository<Supplier>, Repository<Supplier>>();
builder.Services.AddScoped<IRepository<Address>, Repository<Address>>();
builder.Services.AddScoped<IRepository<SupplierProduct>, Repository<SupplierProduct>>();
builder.Services.AddScoped<IRepository<SupplierPricing>, Repository<SupplierPricing>>();
builder.Services.AddScoped<IRepository<SupplierPerformance>, Repository<SupplierPerformance>>();

builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ISupplierProductRepository, SupplierProductRepository>();
builder.Services.AddScoped<ISupplierPricingRepository, SupplierPricingRepository>();
builder.Services.AddScoped<ISupplierPerformanceRepository, SupplierPerformanceRepository>();

// Service Registration
builder.Services.AddScoped<ISupplierService, SupplierService.Core.Services.SupplierService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ISupplierProductService, SupplierProductService>();
builder.Services.AddScoped<ISupplierPricingService, SupplierPricingService>();
builder.Services.AddScoped<ISupplierPerformanceService, SupplierPerformanceService>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Supplier Service API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Supplier Service API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
