using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using TransporterService.Core.Interfaces;
using TransporterService.Core.Mappings;
using TransporterService.Core.Validators;
using TransporterService.Infrastructure.Data;
using TransporterService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database Configuration
builder.Services.AddDbContext<TransporterDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=transporter.db"));

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(TransporterProfile));

// FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterTransporterValidator>();

// Repository Registration
builder.Services.AddScoped<ITransporterRepository, TransporterRepository>();
builder.Services.AddScoped<ITransporterContactRepository, TransporterContactRepository>();
builder.Services.AddScoped<ITransporterFleetRepository, TransporterFleetRepository>();
builder.Services.AddScoped<ITransporterDriverRepository, TransporterDriverRepository>();
builder.Services.AddScoped<ITransporterLicenseRepository, TransporterLicenseRepository>();
builder.Services.AddScoped<ITransporterInsuranceRepository, TransporterInsuranceRepository>();
builder.Services.AddScoped<ITransporterContractRepository, TransporterContractRepository>();
builder.Services.AddScoped<ITransporterPerformanceRepository, TransporterPerformanceRepository>();

// Service Registration
builder.Services.AddScoped<ITransporterService, TransporterService.Core.Services.TransporterService>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Transporter Service API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transporter Service API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TransporterDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
