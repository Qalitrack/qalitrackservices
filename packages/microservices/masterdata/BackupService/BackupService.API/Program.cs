using BackupService.Infrastructure.Services;
using BackupService.Infrastructure.Services.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO.Abstractions;
using System.IO.Enumeration;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using BackupService.Infrastructure.Data;
using BackupService.Infrastructure.Repositories;
using Quartz;
using Quartz.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Quartz scheduler
builder.Services.AddSingleton<ISchedulerFactory>(provider => 
    new StdSchedulerFactory());

// Get metadata directory from configuration or use default
var metadataDirectory = builder.Configuration.GetValue<string>("Backup:MetadataDirectory") 
                       ?? Path.Combine(Directory.GetCurrentDirectory(), "backup-metadata");

// Add backup services with proper dependency injection
builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
builder.Services.AddScoped<IBackupCreationService, BackupCreationService>();
builder.Services.AddScoped<IBackupRestoreService, BackupRestoreService>();
builder.Services.AddScoped<IBackupVerificationService, BackupVerificationService>();
builder.Services.AddScoped<IFileSystem, FileSystem>();
builder.Services.AddScoped<IMicroserviceRepository, MicroserviceRepository>();
builder.Services.AddScoped<IMicroService, MicroService>();
// Register JsonBackupMetadataService with the required string parameter
builder.Services.AddScoped<IBackupMetadataService>(provider =>
{
    var fileSystem = provider.GetRequiredService<IFileSystem>();
    var logger = provider.GetRequiredService<ILogger<JsonBackupMetadataService>>();
    return new JsonBackupMetadataService(fileSystem, logger, metadataDirectory);
});

// Database configuration
if (builder.Configuration.GetValue<bool>("UsePostgreSQL"))
{
    builder.Services.AddDbContext<BackupServiceDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Register DatabaseSeeder for dependency injection
builder.Services.AddScoped<DatabaseSeeder>();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backup Service API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the root URL
});

if (app.Environment.IsDevelopment())
{
    // Development-specific middleware can be added here
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created/migrated and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BackupServiceDbContext>();
    
    // Apply migrations
    dbContext.Database.Migrate();
    
    // Seed the database with initial data
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Get the application URLs from configuration
var urls = builder.Configuration["ASPNETCORE_URLS"]?.Split(';') 
          ?? new[] { "https://localhost:7000", "http://localhost:5000" };

// Print the Swagger URL to console
foreach (var url in urls)
{
    var cleanUrl = url.Trim();
    Console.WriteLine("🚀 Backup Service is running!");
    Console.WriteLine($"📚 Swagger UI: {cleanUrl}");
    Console.WriteLine($"🔗 API Documentation: {cleanUrl}/swagger/v1/swagger.json");
    Console.WriteLine("----------------------------------------------------------");
}

app.Run();