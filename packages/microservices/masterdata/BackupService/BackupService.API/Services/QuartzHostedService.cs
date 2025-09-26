using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BackupService.API.Services;

public class QuartzHostedService : IHostedService
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<QuartzHostedService> _logger;

    public QuartzHostedService(IScheduler scheduler, ILogger<QuartzHostedService> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Quartz scheduler...");
        await _scheduler.Start(cancellationToken);
        _logger.LogInformation("Quartz scheduler started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Quartz scheduler...");
        await _scheduler.Shutdown(true, cancellationToken);
        _logger.LogInformation("Quartz scheduler stopped");
    }
}
