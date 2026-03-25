using BackupService.Infrastructure.Services;
using Prometheus;
using BackupService.Infrastructure.Services.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO.Abstractions;
using System.IO.Enumeration;
using BackupService.API.Services;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using BackupService.Infrastructure.Data;
using BackupService.Infrastructure.Repositories;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.Simpl;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for larger file uploads
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1024 * 1024 * 1024; // 1GB
    serverOptions.Limits.MinRequestBodyDataRate = null;
    serverOptions.Limits.MinResponseDataRate = null;
});

// Configure request limits
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 1024; // 1GB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024 * 1024 * 1024; // 1GB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.BufferBodyLengthLimit = 1024 * 1024 * 1024; // 1GB
    options.MemoryBufferThreshold = 1024 * 1024 * 10; // 10MB
});

// Add Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration - MOVE THIS BEFORE Quartz configuration
if (builder.Configuration.GetValue<bool>("UsePostgreSQL"))
{
    builder.Services.AddDbContext<BackupServiceDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configure Quartz with proper dependency injection
builder.Services.AddQuartz(q =>
{
    // Use Microsoft DI container for job creation - THIS IS KEY
    q.UseMicrosoftDependencyInjectionJobFactory();
    
    // Configure the job store ONLY if you want persistence
    // If you don't need job persistence across restarts, comment out this block
    q.UsePersistentStore(options =>
    {
        options.UsePostgres(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.UseNewtonsoftJsonSerializer(); 
        options.UseClustering(); // optional if you want multiple nodes
    });
    
    // Configure thread pool
    q.UseThreadPool<DefaultThreadPool>(options =>
    {
        options.MaxConcurrency = 10;
    });
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(options =>
{
    // When shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
    options.StartDelay = TimeSpan.FromSeconds(5);
});

// REMOVE these problematic registrations:
// Don't register ISchedulerFactory or IScheduler manually when using AddQuartz()
// The framework handles this automatically

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

// Serve static files including documentation
app.UseStaticFiles();

// Configure documentation serving
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs")),
    RequestPath = "/docs"
});

// Documentation default route
app.MapGet("/docs", () => Results.Redirect("/docs/index.html"));
app.MapFallback("/docs/{**path}", async context =>
{
    var path = context.Request.Path.Value?.Replace("/docs/", "") ?? "index.html";
    if (string.IsNullOrEmpty(path) || path == "/")
        path = "index.html";
    
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs", path);
    if (File.Exists(filePath))
    {
        // Set proper Content-Type based on file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".yml" or ".yaml" => "text/yaml; charset=utf-8",
            _ => "application/octet-stream"
        };
        
        context.Response.ContentType = contentType;
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Documentation file not found");
    }
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
app.UseHttpMetrics();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();

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

app.Run();