using BackupService.Infrastructure.Services;
using BackupService.Infrastructure.Services.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using BackupService.Infrastructure.Data;
using BackupService.Infrastructure.Messaging.Configurations;
using BackupService.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add MassTransit with RabbitMQ
builder.Services.AddMessaging(builder.Configuration);
// Configure options
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
//add  backupinitiation service
builder.Services.AddScoped<IBackupInitiationService, BackupInitiationService>();
builder.Services.AddScoped<IBackupOperationRepository, BackupOperationRepository>();

// Database configuration
if (builder.Configuration.GetValue<bool>("UsePostgreSQL"))
{
    builder.Services.AddDbContext<BackupServiceDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Register repositories

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
app.UseSwaggerUI();
if (app.Environment.IsDevelopment())
{
    // The existing development-specific middleware can remain here
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

// Ensure database is created/migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BackupServiceDbContext>();
    dbContext.Database.Migrate();
}

app.Run();