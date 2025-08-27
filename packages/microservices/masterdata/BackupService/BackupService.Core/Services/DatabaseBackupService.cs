using System.IO.Abstractions;
using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
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
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DatabaseBackupService> _logger;

    public DatabaseBackupService(
        IMicroserviceRepository microserviceRepository,
        IBackupCreationService creationService,
        IBackupMetadataService metadataService,
        IBackupRestoreService restorationService,
        IBackupVerificationService verificationService,
        ISchedulerFactory schedulerFactory,
        IFileSystem fileSystem,
        ILogger<DatabaseBackupService> logger)
    {
        _microserviceRepository = microserviceRepository;
        _creationService = creationService;
        _metadataService = metadataService;
        _restorationService = restorationService;
        _verificationService = verificationService;
        _schedulerFactory = schedulerFactory;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<BackupResult> CreateBackupAsync(BackupType type, string microservice, string saveLocation, string? cronSchedule = null, CancellationToken ct = default)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
            ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        if (ms.Status != MicroserviceStatus.Active)
        {
            _logger.LogWarning("Microservice {Name} is {Status}, cannot backup", microservice, ms.Status);
            throw new InvalidOperationException($"Microservice {microservice} is not Active");
        }

        var result = await _creationService.CreateBackupAsync(type, microservice, saveLocation, ct);

        if (!string.IsNullOrEmpty(cronSchedule))
            await ScheduleBackupAsync(microservice, type, cronSchedule, saveLocation, ct);

        return result;
    }

    public async Task<RestoreResult> RestoreBackupAsync(string microservice, string backupFilePath, 
        CancellationToken ct = default)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
                 ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        // Validate required parameters
        if (string.IsNullOrWhiteSpace(backupFilePath))
            throw new ArgumentException("Backup file path must be provided", nameof(backupFilePath));

        if (!File.Exists(backupFilePath))
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
            var jobKey = new JobKey($"{microservice}-{type}", "backup-jobs");
            var triggerKey = new TriggerKey($"{microservice}-{type}-trigger", "backup-triggers");

            await scheduler.UnscheduleJob(triggerKey, ct);
            var result = await scheduler.DeleteJob(jobKey, ct);

            if (result)
                _logger.LogInformation("Unscheduled {BackupType} backup for {Microservice}", type, microservice);

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
                                NextFireTime = trigger.GetNextFireTimeUtc()?.UtcDateTime,
                                SaveLocation = jobDetail.JobDataMap.GetString("SaveLocation")!
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

    private async Task ScheduleBackupAsync(string microservice, BackupType type, string cronSchedule, string saveLocation, CancellationToken ct)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var job = JobBuilder.Create<BackupJob>()
                .WithIdentity($"{microservice}-{type}", "backup-jobs")
                .UsingJobData(new JobDataMap
                {
                    ["Microservice"] = microservice,
                    ["Type"] = type.ToString(),
                    ["SaveLocation"] = saveLocation
                })
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{microservice}-{type}-trigger", "backup-triggers")
                .WithCronSchedule(cronSchedule)
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(job, trigger, ct);
            _logger.LogInformation("Scheduled {BackupType} backup for {Microservice} with cron {Cron}", type, microservice, cronSchedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule backup for {Microservice}", microservice);
            throw;
        }
    }
    
    public class BackupJob : IJob
    {
        private readonly IServiceProvider _provider;

        public BackupJob(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
            var data = context.JobDetail.JobDataMap;
            var microservice = data.GetString("Microservice")!;
            var type = Enum.Parse<BackupType>(data.GetString("Type")!);
            var saveLocation = data.GetString("SaveLocation")!;

            try
            {
                await service.CreateBackupAsync(type, microservice, saveLocation, null, context.CancellationToken);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<BackupJob>>();
                logger.LogError(ex, "Scheduled backup failed for {Microservice}", microservice);
                throw new JobExecutionException(ex, true);
            }
        }
    }
}