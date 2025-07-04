using ArchiveService.Infrastructure.Extensions;
using ArchiveService.Api.BackgroundServices;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/archive-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add Infrastructure services (includes DbContext, Repositories, and Core services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Background Services
builder.Services.AddHostedService<RetentionPolicyBackgroundService>();
builder.Services.AddHostedService<ArchiveMaintenanceBackgroundService>();

// Configure API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArchiveService.Infrastructure.Data.ArchiveDbContext>();
    context.Database.EnsureCreated();
}

Log.Information("Archive Service API starting up...");

app.Run();

Log.Information("Archive Service API shutting down...");
Log.CloseAndFlush();