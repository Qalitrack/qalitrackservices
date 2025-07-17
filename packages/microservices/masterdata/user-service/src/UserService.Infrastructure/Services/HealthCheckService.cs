using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Core.Interfaces;
using UserService.Core.Options;
using HealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

namespace UserService.Infrastructure.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IOptions<BackupOptions> _backupOptions;
        private readonly TimeSpan _maxBackupAge = TimeSpan.FromDays(7); // Consider making this configurable

        public HealthCheckService(
            ILogger<HealthCheckService> logger,
            IOptions<BackupOptions> backupOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backupOptions = backupOptions ?? throw new ArgumentNullException(nameof(backupOptions));
        }

        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var backupPath = _backupOptions.Value?.Path;
                if (string.IsNullOrEmpty(backupPath))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("Backup path is not configured"));
                }

                if (!Directory.Exists(backupPath))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy($"Backup directory does not exist: {backupPath}"));
                }

                var backupFiles = Directory.GetFiles(backupPath, "*.bak");
                if (backupFiles.Length == 0)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("No backup files found"));
                }

                var mostRecentBackup = backupFiles
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .First();

                var age = DateTime.UtcNow - mostRecentBackup.LastWriteTimeUtc;
                if (age > _maxBackupAge)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy($"Most recent backup is too old: {age.TotalDays:F1} days"));
                }

                return Task.FromResult(HealthCheckResult.Healthy("Backup health check passed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking backup health status");
                return Task.FromResult(HealthCheckResult.Unhealthy("Error checking backup health status", ex));
            }
        }
    }
}
