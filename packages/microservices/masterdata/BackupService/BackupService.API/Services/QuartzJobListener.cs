using Quartz;
using Microsoft.Extensions.Logging;

namespace BackupService.API.Services;

public class QuartzJobListener : IJobListener
{
    private readonly ILogger<QuartzJobListener> _logger;

    public QuartzJobListener(ILogger<QuartzJobListener> logger)
    {
        _logger = logger;
    }

    public string Name => "QuartzJobListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Job {JobName} is about to be executed", context.JobDetail.Key);
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Job {JobName} execution was vetoed", context.JobDetail.Key);
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        if (jobException != null)
        {
            _logger.LogError(jobException, "Job {JobName} failed with exception", context.JobDetail.Key);
        }
        else
        {
            _logger.LogInformation("Job {JobName} completed successfully", context.JobDetail.Key);
        }
        return Task.CompletedTask;
    }
}
