using System.IO.Abstractions;
using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;

namespace BackupService.Core.Services;

public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly IMicroserviceRepository _microserviceRepository;
    private readonly IBackupCreationService _creationService;
    private readonly IBackupMetadataService _metadataService;
    private readonly IBackupRestoreService _restorationService;
    private readonly IBackupVerificationService _verificationService;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IConfiguration _configuration;

    public DatabaseBackupService(
        IMicroserviceRepository microserviceRepository,
        IBackupCreationService creationService,
        IBackupMetadataService metadataService,
        IBackupRestoreService restorationService,
        IBackupVerificationService verificationService,
        IConfiguration configuration,
        ISchedulerFactory schedulerFactory,
        IServiceProvider serviceProvider,
        ILogger<DatabaseBackupService> logger,
        IFileSystem fileSystem)
    {
        _microserviceRepository = microserviceRepository;
        _creationService = creationService;
        _metadataService = metadataService;
        _restorationService = restorationService;
        _verificationService = verificationService;
        _schedulerFactory = schedulerFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _fileSystem = fileSystem;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<BackupResult> CreateBackupAsync(BackupType type, string microservice, string? cronSchedule = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Received backup request - Type: {BackupType}, Microservice: {Microservice}, CronSchedule: {CronSchedule}", type, microservice, cronSchedule ?? "None (immediate backup)");
        
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
            ?? throw new KeyNotFoundException($"Microservice {microservice} not found");
            
        if (ms.Status != MicroserviceStatus.Active)
        {
            _logger.LogWarning("Microservice {Name} is {Status}, cannot backup", microservice, ms.Status);
            throw new InvalidOperationException($"Microservice {microservice} is not Active");
        }

        // If cron schedule is provided, only schedule the job without creating an immediate backup
        if (!string.IsNullOrEmpty(cronSchedule))
        {
            await ScheduleBackupAsync(microservice, type, cronSchedule, ct);
            _logger.LogInformation("Successfully scheduled backup job");
            return new BackupResult
            {
                Success = true,
                Message = $"Scheduled {type} backup for {microservice} with cron expression: {cronSchedule}",
                BackupType = type,
                Timestamp = DateTime.UtcNow,
                ServiceName = microservice,
                IsValid = true
            };
        }
        else
        {
            _logger.LogInformation("No cron schedule provided, performing immediate backup");
        }

        // If no cron schedule, perform an immediate backup
        return await _creationService.CreateBackupAsync(type, microservice, ct);
    }

    public async Task<RestoreResult> RestoreBackupAsync(string microservice, string backupFilePath, 
        CancellationToken ct = default)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
                 ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        // Validate required parameters
        if (string.IsNullOrWhiteSpace(backupFilePath))
            throw new ArgumentException("Backup file path must be provided", nameof(backupFilePath));

        // pgBackRest identifiers ("pgbackrest:{label}") aren't real files on this
        // container's filesystem — only legacy pg_dump paths get existence-checked here.
        if (!BackupRestoreService.IsPgBackRestIdentifier(backupFilePath) && !File.Exists(backupFilePath))
            throw new FileNotFoundException($"Backup file not found: {backupFilePath}");

        _logger.LogInformation("Starting restore for {Microservice} from backup file {BackupFilePath}", 
            microservice, backupFilePath);

        // Extract backup ID from filename for logging/tracking
        var backupId = Path.GetFileNameWithoutExtension(backupFilePath);

        // Call the actual restoration service directly with the backup file path
        var result = await _restorationService.RestoreBackupAsync(microservice, backupFilePath, ct);

        _logger.LogInformation("Restore completed successfully for {Microservice} from backup {BackupId}", 
            microservice, backupId);

        return result;
    }
    

    public async Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? microservice = null, CancellationToken ct = default)
    {
        return await _metadataService.GetAvailableBackupsAsync(microservice, ct);
    }

    

    public async Task ValidateAllBackupsAsync(string? microservice = null, CancellationToken ct = default)
    {
        var backups = await _metadataService.GetAvailableBackupsAsync(microservice, ct);

        foreach (var backup in backups)
        {
            try
            {
                await _verificationService.VerifyBackupIntegrityAsync(backup.FileName, backup.BackupType, ct);
                _logger.LogInformation("Backup {FileName} validated", backup.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Backup {Path} failed validation", backup.FileName);
            }
        }
    }

    public async Task<BackupStatistics> GetBackupStatisticsAsync(string? microservice = null, CancellationToken ct = default)
    {
        var chains = await _metadataService.GetAllBackupChainsAsync(microservice, ct);
        var backups = await _metadataService.GetAvailableBackupsAsync(microservice, ct);

        var now = DateTime.UtcNow;
        var last30Days = now.AddDays(-30);

        var recentBackups = backups.Where(b => b.CreatedAt >= last30Days).ToList();
        if (!recentBackups.Any())
        {
            throw new KeyNotFoundException($"No backups found for microservice: {microservice}");
        }

        return new BackupStatistics
        {
            GeneratedAt = now,
            TotalChains = chains.Count,
            TotalBackupFiles = recentBackups.Count,
            TotalFullBackups = recentBackups.Count(b => b.BackupType == BackupType.Full),
            TotalSizeBytes = recentBackups.Sum(b => b.FileSizeBytes),
            OldestBackup = recentBackups.Min(b => b.CreatedAt),
            NewestBackup = recentBackups.Max(b => b.CreatedAt),
            ValidChains = chains.Count
        };
    }

    public async Task<bool> UnscheduleBackupAsync(string microservice, BackupType type, CancellationToken ct = default)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler(ct);
            
            // Match the job key format used in ScheduleBackupAsync
            var jobKey = new JobKey($"{microservice}-{type}-backup", "backupGroup");
            var triggerKey = new TriggerKey($"{microservice}-{type}-trigger", "backupTriggers");

            _logger.LogInformation("Attempting to unschedule job with key: {JobKey} in group: {Group}", jobKey.Name, jobKey.Group);

            // First check if the job exists
            bool jobExists = await scheduler.CheckExists(jobKey, ct);
            if (!jobExists)
            {
                _logger.LogInformation("No scheduled {BackupType} backup found for {Microservice} (JobKey: {JobKey}, Group: {Group})", 
                    type, microservice, jobKey.Name, jobKey.Group);
                return false;
            }

            // Check if the trigger exists before trying to unschedule it
            bool triggerExists = await scheduler.CheckExists(triggerKey, ct);
            if (triggerExists)
            {
                _logger.LogDebug("Unscheduling trigger: {TriggerKey}", triggerKey);
                await scheduler.UnscheduleJob(triggerKey, ct);
            }
            else
            {
                _logger.LogDebug("No trigger found with key: {TriggerKey}", triggerKey);
            }

            // Delete the job
            _logger.LogDebug("Deleting job: {JobKey}", jobKey);
            bool result = await scheduler.DeleteJob(jobKey, ct);

            if (result)
            {
                _logger.LogInformation("Successfully unscheduled {BackupType} backup for {Microservice}", type, microservice);
            }
            else
            {
                _logger.LogWarning("Failed to delete job for {BackupType} backup of {Microservice}", type, microservice);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unschedule backup for {Microservice}", microservice);
            return false;
        }
    }

    public async Task<List<ScheduledBackup>> GetScheduledBackupsAsync(CancellationToken ct = default)
    {
        var scheduledBackups = new List<ScheduledBackup>();

        try
        {
            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), ct);

            foreach (var jobKey in jobKeys)
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey, ct);
                if (jobDetail != null && jobDetail.JobDataMap.ContainsKey("Microservice"))
                {
                    var triggers = await scheduler.GetTriggersOfJob(jobKey, ct);
                    foreach (var trigger in triggers)
                    {
                        if (trigger is ICronTrigger cronTrigger)
                        {
                            scheduledBackups.Add(new ScheduledBackup
                            {
                                Microservice = jobDetail.JobDataMap.GetString("Microservice")!,
                                BackupType = Enum.Parse<BackupType>(jobDetail.JobDataMap.GetString("Type")!),
                                CronSchedule = cronTrigger.CronExpressionString,
                                NextFireTime = trigger.GetNextFireTimeUtc()?.UtcDateTime
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve scheduled backups");
        }

        return scheduledBackups;
    }

    private bool IsValidCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            return false;

        try
        {
            // Use a simple regex to validate the cron expression format
            // This checks for the standard 5 or 6 field cron format
            var cronRegex = new System.Text.RegularExpressions.Regex(
                @"^([0-9]|,|\*|\/|-|\?|L|W|#)+\s+([0-9]|,|\*|\/|-|\?|L|W|#)+\s+([0-9]|,|\*|\/|-|\?|L|W|#)+\s+([0-9]|,|\*|\/|-|\?|L|W|#)+\s+([0-9]|,|\*|\/|-|\?|L|W|#)+(\s+([0-9]|,|\*|\/|-|\?|L|W|#)+)?$");
            
            if (!cronRegex.IsMatch(cronExpression))
            {
                _logger.LogDebug("Cron expression does not match required format: {CronExpression}", cronExpression);
                return false;
            }
            
            // If we got here, the format is valid
            _logger.LogDebug("Cron expression is valid: {CronExpression}", cronExpression);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating cron expression: {CronExpression}", cronExpression);
            return false;
        }
    }

    private async Task ScheduleBackupAsync(string microservice, BackupType type, string cronSchedule, CancellationToken ct)
{
    _logger.LogInformation("Entering ScheduleBackupAsync - Microservice: {Microservice}, Type: {Type}", microservice, type);

    try
    {
        _logger.LogInformation("Using cron expression: {CronExpression}", cronSchedule);
        
        if (string.IsNullOrWhiteSpace(cronSchedule))
        {
            throw new ArgumentException("Cron schedule cannot be null or empty");
        }

        cronSchedule = cronSchedule.Trim();
        _logger.LogInformation("Trimmed cron expression: {CronExpression}", cronSchedule);

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var saveLocation = _configuration["BackupSettings:StoragePath"];
        _logger.LogInformation("Creating job with save location: {SaveLocation}", saveLocation);

        // Create a unique job key for this backup schedule
        var jobKey = new JobKey($"{microservice}-{type}-backup", "backupGroup");
        var triggerKey = new TriggerKey($"{microservice}-{type}-trigger", "backupTriggers");

        var jobData = new JobDataMap
        {
            ["Microservice"] = microservice,
            ["Type"] = type.ToString(),
            ["SaveLocation"] = saveLocation ?? string.Empty
        };

        _logger.LogInformation("Job data: {JobData}", string.Join("; ", jobData.Select(kv => $"{kv.Key}={kv.Value}")));

        // Check if the job already exists
        var jobExists = await scheduler.CheckExists(jobKey, ct);
        if (jobExists)
        {
            _logger.LogInformation("Job already exists, updating schedule");
            await scheduler.DeleteJob(jobKey, ct);
        }

        // Create job detail
        var job = JobBuilder.Create<BackupJob>()
            .WithIdentity(jobKey)
            .UsingJobData(jobData)
            .StoreDurably()
            .Build();

        _logger.LogInformation("Creating trigger with cron expression: {Cron}", cronSchedule);

        try
        {
            // Create trigger with the cron schedule
            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .WithCronSchedule(cronSchedule, x => x
                    .InTimeZone(TimeZoneInfo.Utc)
                    .WithMisfireHandlingInstructionDoNothing())
                .StartNow()
                .Build();

            // Schedule the job with the trigger
            await scheduler.ScheduleJob(job, trigger, ct);
            
            // Start the scheduler if it's not already started
            if (!scheduler.IsStarted)
            {
                await scheduler.Start(ct);
            }

            var nextFireTime = trigger.GetNextFireTimeUtc();
            _logger.LogInformation("Successfully scheduled {BackupType} backup for {Microservice} with cron {Cron}. Next execution: {NextExecutionTime}",
                type, microservice, cronSchedule, nextFireTime?.UtcDateTime);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Failed to parse cron expression: {CronExpression}", cronSchedule);
            throw new ArgumentException($"Invalid cron expression format: {cronSchedule}. Please ensure it follows the standard cron format with 6-7 fields.", ex);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to schedule backup for {Microservice}", microservice);
        throw;
    }
}
    
    public class BackupJob : IJob
    {
        private readonly IBackupCreationService _creationService;
        private readonly ILogger<BackupJob> _logger;

        public BackupJob(IBackupCreationService creationService, ILogger<BackupJob> logger)
        {
            _creationService = creationService;
            _logger = logger;
        }

       public async Task Execute(IJobExecutionContext context)
{
    var jobName = context.JobDetail.Key.Name;
    var data = context.JobDetail.JobDataMap;
    var microservice = data.GetString("Microservice") ?? string.Empty;
    var type = Enum.Parse<BackupType>(data.GetString("Type") ?? "Full");
    
    _logger.LogInformation("Starting {BackupType} backup job for {Microservice} (Job: {JobName})", 
        type, microservice, jobName);

    try
    {
        _logger.LogDebug("Job data: {JobData}", 
            string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}")));
        _logger.LogInformation("Initiating backup creation...");

        // Execute the backup directly using the injected service
        _logger.LogInformation("Starting backup creation...");
        var result = await _creationService.CreateBackupAsync(type, microservice, context.CancellationToken);
    
        _logger.LogInformation("Backup result - Success: {Success}, Message: {Message}", 
            result.Success, result.Message);

        // Always log the result but don't fail the job
        if (!result.Success)
        {
            _logger.LogWarning("Backup job completed with warnings: {Message}", result.Message);
        }
        else
        {
            _logger.LogInformation("Backup job completed successfully: {Message}", result.Message);
        }
        
        // Always return successfully since the backup file was created
        return;
    }
    catch (Exception ex)
    {
        var errorMsg = $"Critical error in backup job {jobName} for {microservice} ({type}): {ex.Message}";
        _logger.LogError(ex, errorMsg);
        
        // Log the full exception details including inner exceptions
        var currentEx = ex;
        while (currentEx != null)
        {
            _logger.LogError("Exception details - Type: {Type}, Message: {Message}, Stack: {Stack}", 
                currentEx.GetType().Name, 
                currentEx.Message, 
                currentEx.StackTrace);
            currentEx = currentEx.InnerException;
        }

        var jobEx = new JobExecutionException(errorMsg, ex, false);
        jobEx.UnscheduleFiringTrigger = false; // Don't unschedule the trigger on failure
        jobEx.UnscheduleAllTriggers = false;   // Don't unschedule all triggers
        throw jobEx;
    }
}
    }
}