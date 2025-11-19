using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.BackgroundServices;

public class PerformanceMetricsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PerformanceMetricsBackgroundService> _logger;
    private Timer? _timer;

    public PerformanceMetricsBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PerformanceMetricsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Performance Metrics Background Service is starting");

        // Calculate time until next Sunday at 11:59 PM
        var now = DateTime.Now;
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && now.TimeOfDay > new TimeSpan(23, 59, 0))
        {
            daysUntilSunday = 7;
        }

        var nextSunday = now.Date.AddDays(daysUntilSunday).AddHours(23).AddMinutes(59);
        var timeUntilNextSunday = nextSunday - now;

        // Start timer - first run at next Sunday 11:59 PM, then every 7 days
        _timer = new Timer(
            DoWork,
            null,
            timeUntilNextSunday,
            TimeSpan.FromDays(7));

        _logger.LogInformation("Performance Metrics will run in {hours} hours at {time}",
            timeUntilNextSunday.TotalHours, nextSunday);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Performance Metrics Background Service is calculating metrics at {time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var performanceMetricsService = scope.ServiceProvider.GetRequiredService<IPerformanceMetricsService>();
            var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

            // Get all unique technician IDs
            var allAssignments = await assignmentService.GetPagedAsync(1, 10000);
            var technicianIds = allAssignments.Items
                .SelectMany(a => a.TechnicianIds)
                .Distinct()
                .ToList();

            _logger.LogInformation("Found {Count} unique technicians to calculate metrics for", technicianIds.Count);

            // Calculate metrics for the past week
            var periodEnd = DateTime.UtcNow;
            var periodStart = periodEnd.AddDays(-7);

            foreach (var technicianId in technicianIds)
            {
                try
                {
                    var metrics = await performanceMetricsService.CalculateMetricsAsync(
                        technicianId,
                        periodStart,
                        periodEnd);

                    _logger.LogInformation(
                        "Calculated weekly metrics for technician {technicianId}: Score {score}%, OnTime {onTimeRate}%, Alert: {alertLevel}",
                        technicianId,
                        metrics.PerformanceScore,
                        metrics.OnTimeRate,
                        metrics.AlertLevel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating metrics for technician {technicianId}", technicianId);
                }
            }

            _logger.LogInformation("Performance Metrics Background Service completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Performance Metrics Background Service");
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
