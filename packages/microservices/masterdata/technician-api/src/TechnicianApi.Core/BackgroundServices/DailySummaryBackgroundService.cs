using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.BackgroundServices;

public class DailySummaryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailySummaryBackgroundService> _logger;
    private Timer? _timer;

    public DailySummaryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DailySummaryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Summary Background Service is starting");

        // Calculate time until next midnight
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1).AddMinutes(-1); // 11:59 PM
        var timeUntilMidnight = nextMidnight - now;

        // Start timer - first run at 11:59 PM, then every 24 hours
        _timer = new Timer(
            DoWork,
            null,
            timeUntilMidnight,
            TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Daily Summary Background Service is generating summaries at {time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dailySummaryService = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();
            var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

            // Get all unique technician IDs from assignments
            var allAssignments = await assignmentService.GetPagedAsync(1, 10000);
            var technicianIds = allAssignments.Items
                .SelectMany(a => a.TechnicianIds)
                .Distinct()
                .ToList();

            _logger.LogInformation("Found {Count} unique technicians to generate summaries for", technicianIds.Count);

            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            foreach (var technicianId in technicianIds)
            {
                try
                {
                    var summary = await dailySummaryService.GenerateSummaryAsync(technicianId, yesterday);
                    _logger.LogInformation(
                        "Generated daily summary for technician {technicianId}: {completedTasks} completed, {delayedTasks} delayed, Alert: {alertLevel}",
                        technicianId,
                        summary.CompletedTasks,
                        summary.DelayedTasks,
                        summary.AlertLevel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating summary for technician {technicianId}", technicianId);
                }
            }

            _logger.LogInformation("Daily Summary Background Service completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Daily Summary Background Service");
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
