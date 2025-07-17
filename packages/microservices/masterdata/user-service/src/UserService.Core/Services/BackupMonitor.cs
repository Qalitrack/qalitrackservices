using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class BackupMonitor : BackgroundService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupMonitor> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public BackupMonitor(
        IServiceScopeFactory scopeFactory,
        ILogger<BackupMonitor> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup Monitor Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var healthCheckService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();
                    var result = await healthCheckService.CheckHealthAsync(stoppingToken);

                    if (result.Status == HealthStatus.Unhealthy)
                    {
                        _logger.LogError("Backup health check failed: {StatusDescription}", 
                            result.Description ?? "No details available");
                        // Trigger alert (e.g., send email, SMS, etc.)
                    }
                    else if (result.Status == HealthStatus.Degraded)
                    {
                        _logger.LogWarning("Backup health check degraded: {StatusDescription}", 
                            result.Description ?? "No details available");
                        // Trigger warning
                    }
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                _logger.LogInformation("Backup Monitor Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring backup health");
                // Wait before retrying to prevent tight loop on failure
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Backup Monitor Service is stopping.");
        await base.StopAsync(cancellationToken);
    }
}