// using System.IO;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Diagnostics.HealthChecks;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using UserService.Core.Interfaces;
// using UserService.Core.Options;
//
// namespace UserService.Core.Services;
//
// public class BackupHealthCheck : IHealthCheck
// {
//     private readonly ILogger<BackupHealthCheck> _logger;
//     private readonly BackupOptions _options;
//     private readonly IDatabaseBackupService _backupService;
//
//     public BackupHealthCheck(
//         IOptions<BackupOptions> options,
//         ILogger<BackupHealthCheck> logger,
//         IDatabaseBackupService backupService)
//     {
//         _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
//     }
//
//     public async Task<HealthCheckResult> CheckHealthAsync(
//         HealthCheckContext context, 
//         CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             // Ensure backup directory exists
//             if (!Directory.Exists(_options.Path))
//             {
//                 _logger.LogWarning("Backup directory does not exist: {BackupPath}", _options.Path);
//                 return HealthCheckResult.Unhealthy($"Backup directory not found: {_options.Path}");
//             }
//             
//             // Check for existence of at least one full backup
//             var fullBackups = Directory.GetFiles(_options.Path, "UserService_Full_*.db")
//                 .Select(f => new FileInfo(f))
//                 .OrderByDescending(f => f.LastWriteTimeUtc)
//                 .ToList();
//                 
//             if (fullBackups.Count == 0)
//             {
//                 _logger.LogWarning("No full backups found in {BackupPath}", _options.Path);
//                 return HealthCheckResult.Unhealthy("No full backups found");
//             }
//             
//             var latestFullBackup = fullBackups[0];
//             var timeSinceLastFullBackup = DateTime.UtcNow - latestFullBackup.LastWriteTimeUtc;
//             
//             // Check if full backup is too old
//             if (timeSinceLastFullBackup > _options.FullBackupInterval + TimeSpan.FromDays(1))
//             {
//                 _logger.LogWarning(
//                     "Last full backup is {DaysOld} days old (older than configured interval of {IntervalDays} days)",
//                     timeSinceLastFullBackup.TotalDays,
//                     _options.FullBackupInterval.TotalDays);
//                     
//                 return HealthCheckResult.Degraded(
//                     $"Last full backup is {timeSinceLastFullBackup.TotalDays:0.0} days old");
//             }
//             
//             // Check for recent incremental backups if full backup is old
//             if (timeSinceLastFullBackup > _options.IncrementalBackupInterval)
//             {
//                 var incrementalBackups = Directory.GetFiles(_options.Path, "UserService_Incremental_*.db")
//                     .Select(f => new FileInfo(f))
//                     .OrderByDescending(f => f.LastWriteTimeUtc)
//                     .ToList();
//                     
//                 var latestIncremental = incrementalBackups.FirstOrDefault();
//                 
//                 if (latestIncremental == null || 
//                     (DateTime.UtcNow - latestIncremental.LastWriteTimeUtc) > _options.IncrementalBackupInterval + TimeSpan.FromHours(1))
//                 {
//                     _logger.LogWarning(
//                         "No recent incremental backups found. Last was {LastIncrementalTime} (expected within {IntervalHours} hours)",
//                         latestIncremental?.LastWriteTimeUtc,
//                         _options.IncrementalBackupInterval.TotalHours);
//                         
//                     return HealthCheckResult.Degraded("No recent incremental backups found");
//                 }
//             }
//             
//             // Verify backup file integrity
//             try
//             {
//                 await _backupService.VerifyBackupIntegrityAsync(latestFullBackup.FullName, cancellationToken);
//                 _logger.LogDebug("Backup health check passed successfully");
//                 return HealthCheckResult.Healthy();
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Backup integrity check failed for {BackupFile}", latestFullBackup.Name);
//                 return HealthCheckResult.Unhealthy("Backup integrity check failed", ex);
//             }
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error checking backup health");
//             return HealthCheckResult.Unhealthy("Error checking backup health", ex);
//         }
//     }
// }