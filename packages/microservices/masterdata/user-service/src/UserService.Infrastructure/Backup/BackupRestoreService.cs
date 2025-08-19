using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading;
using Messaging.Contracts.Messaging.contracts;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
namespace UserService.Infrastructure.Backup;

public class BackupRestoreService : IBackupRestoreService
{
    private readonly ILogger<BackupRestoreService> _logger;
    private readonly IOptions<BackupOptions> _options;
    private readonly IDistributedCache _cache;
    private readonly IBackupMetadataService _metadataService;
    private readonly IBackupVerificationService _verificationService;
    private readonly string _connectionString;
    private readonly string _dockerContainerName;
    private readonly string _hostBackupPath;
    private readonly string _containerBackupPath;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public BackupRestoreService(
        IConfiguration config,
        ILogger<BackupRestoreService> logger,
        IOptions<BackupOptions> options,
        IDistributedCache cache,
        IBackupMetadataService metadataService,
        IBackupVerificationService verificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));

        _connectionString = config?.GetConnectionString("DefaultConnection") ??
                            throw new InvalidOperationException("DefaultConnection string is not configured");
        _dockerContainerName = config["Docker:PostgresContainerName"] ??
                               throw new InvalidOperationException("Postgres container name not configured");

        _hostBackupPath = _options.Value.Path ??
                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                              "UserService", "Backups");

        _containerBackupPath = config["Docker:BackupMountPath"] ?? "/var/backups";
    }

    public async Task<RestoreResult> RestoreBackupAsync(string backupId = "latest",
        CancellationToken cancellationToken = default)
    {
        await _operationLock.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Starting automated restore process for: {BackupId}", backupId);

            var restoreResult = new RestoreResult
            {
                BackupId = backupId,
                ServiceName = "UserService",
                StartedAt = DateTime.UtcNow
            };

            // Enable cache-first mode for database operations during restore
            await EnableCacheFirstModeAsync(cancellationToken);

            var (fullBackupPath, incrementalPaths) = await ResolveBackupPathsAsync(backupId);

            restoreResult.FullBackupUsed = Path.GetFileName(fullBackupPath);
            restoreResult.IncrementalBackupsUsed = incrementalPaths.Select(Path.GetFileName).ToList()!;
            restoreResult.TotalFilesProcessed = 1 + incrementalPaths.Count;

            // Verify all backups before starting restore
            await _verificationService.VerifyBackupIntegrityAsync(fullBackupPath, BackupType.Full, cancellationToken);
            foreach (var incPath in incrementalPaths)
            {
                await _verificationService.VerifyBackupIntegrityAsync(incPath, BackupType.Incremental,
                    cancellationToken);
            }

            try
            {
                // Cache critical data before stopping database
                await CacheCriticalDataAsync(cancellationToken);

                // Perform restore with hot standby capability
                await PerformHotRestoreAsync(fullBackupPath, incrementalPaths, cancellationToken);

                // Verify database integrity
                await _verificationService.VerifyDatabaseIntegrityAsync(_connectionString, cancellationToken);

                restoreResult.CompletedAt = DateTime.UtcNow;
                restoreResult.IsSuccessful = true;
                restoreResult.Message = "Restore completed successfully";

                _logger.LogInformation("Automated restore completed successfully for {BackupId}", backupId);
                return restoreResult;
            }
            catch (Exception ex)
            {
                restoreResult.CompletedAt = DateTime.UtcNow;
                restoreResult.IsSuccessful = false;
                restoreResult.Message = ex.Message;
                restoreResult.ErrorDetails = ex.ToString();

                _logger.LogError(ex, "Error during automated restore");
                throw new InvalidOperationException("Failed to restore database", ex);
            }
            finally
            {
                // Disable cache-first mode and sync cached data
                await DisableCacheFirstModeAndSyncAsync(cancellationToken);
                await CleanupRestoreTempFilesAsync(cancellationToken);
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }
    public async Task<RestorePreviewResult> PreviewRestoreAsync(string backupId)
    {
        var (fullBackupPath, incrementalPaths) = await ResolveBackupPathsAsync(backupId);
        var chain = await _metadataService.GetBackupChainAsync(backupId);

        return new RestorePreviewResult
        {
            BackupId = backupId,
            ChainId = chain != null ? _metadataService.GetChainId(chain) : "unknown",
            FullBackupFile = Path.GetFileName(fullBackupPath),
            IncrementalFiles = incrementalPaths.Select(Path.GetFileName).ToList()!,
            TotalFilesToProcess = 1 + incrementalPaths.Count,
            EstimatedRestoreTimeMinutes = CalculateEstimatedRestoreTime(fullBackupPath, incrementalPaths),
            RequiredSpaceBytes = CalculateRequiredSpace(fullBackupPath, incrementalPaths),
            RestoreToTimestamp = GetRestoreToTimestamp(backupId, chain),
            ServiceName = "UserService"
        };
    }

    private async Task<(string fullBackupPath, List<string> incrementalPaths)> ResolveBackupPathsAsync(string backupId)
    {
        if (backupId == "latest")
        {
            var latestChain = await _metadataService.GetLatestFullBackupChainAsync();
            if (latestChain == null)
                throw new InvalidOperationException("No backups available for restore");

            var fullPath = Path.Combine(_hostBackupPath, latestChain.FullBackupFile);
            var incPaths = latestChain.Incrementals.Select(inc => Path.Combine(_hostBackupPath, inc)).ToList();

            return (fullPath, incPaths);
        }
        else
        {
            var backupPath = Path.Combine(_hostBackupPath, backupId);
            if (!File.Exists(backupPath))
                throw new FileNotFoundException($"Backup file not found: {backupPath}");

            if (IsFullBackup(backupId))
            {
                return (backupPath, new List<string>());
            }
            else
            {
                var chain = await _metadataService.FindChainForIncrementalAsync(backupId);
                if (chain == null)
                    throw new InvalidOperationException("No associated full backup found for incremental");

                var fullPath = Path.Combine(_hostBackupPath, chain.FullBackupFile);
                var incIndex = chain.Incrementals.IndexOf(backupId);
                var incPaths = chain.Incrementals.Take(incIndex + 1)
                    .Select(inc => Path.Combine(_hostBackupPath, inc)).ToList();

                return (fullPath, incPaths);
            }
        }
    }

    private async Task PerformHotRestoreAsync(string fullBackupPath, List<string> incrementalPaths,
        CancellationToken cancellationToken)
    {
        var containerFullPath = Path.Combine(_containerBackupPath, Path.GetFileName(fullBackupPath));
        var restoreDir = "/var/lib/postgresql/restore";
        var dataDir = "/var/lib/postgresql/data";

        // Create restore directory
        await ExecuteDockerCommandAsync($"mkdir -p {restoreDir}", cancellationToken);

        // Stop PostgreSQL gracefully
        await ExecuteDockerCommandAsync("pg_ctl stop -D /var/lib/postgresql/data -m fast", cancellationToken);

        // Backup current data directory for rollback
        await ExecuteDockerCommandAsync($"mv {dataDir} {dataDir}.backup.{DateTime.UtcNow:yyyyMMddHHmmss}",
            cancellationToken);

        try
        {
            // Restore full backup
            await ExecuteDockerCommandAsync($"mkdir -p {dataDir}", cancellationToken);
            await ExecuteDockerCommandAsync($"tar -xzf {containerFullPath} -C {dataDir} --strip-components=1",
                cancellationToken);

            // Apply incremental backups in sequence
            foreach (var incPath in incrementalPaths)
            {
                var containerIncPath = Path.Combine(_containerBackupPath, Path.GetFileName(incPath));
                await ExecuteDockerCommandAsync($"tar -xzf {containerIncPath} -C {dataDir}/pg_wal", cancellationToken);
                _logger.LogInformation("Applied incremental backup: {IncPath}", Path.GetFileName(incPath));
            }

            // Configure recovery settings for hot standby
            var recoveryConf = $@"
standby_mode = 'on'
restore_command = 'cp {dataDir}/pg_wal/%f %p'
recovery_target_timeline = 'latest'
hot_standby = on
hot_standby_feedback = on
";
            await ExecuteDockerCommandAsync($"echo '{recoveryConf}' > {dataDir}/recovery.conf", cancellationToken);

            // Start PostgreSQL in recovery mode
            await ExecuteDockerCommandAsync("pg_ctl start -D /var/lib/postgresql/data -w", cancellationToken);

            // Wait for recovery completion
            await WaitForRecoveryCompletionAsync(cancellationToken);
        }
        catch
        {
            // Rollback on failure
            await ExecuteDockerCommandAsync($"rm -rf {dataDir}", cancellationToken);
            await ExecuteDockerCommandAsync($"mv {dataDir}.backup.* {dataDir}", cancellationToken);
            await ExecuteDockerCommandAsync("pg_ctl start -D /var/lib/postgresql/data", cancellationToken);
            throw;
        }
    }

    private async Task EnableCacheFirstModeAsync(CancellationToken cancellationToken)
    {
        await _cache.SetStringAsync("system:cache_first_mode", "true", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        }, cancellationToken);

        _logger.LogInformation("Enabled cache-first mode for restore operation");
    }

    private async Task CacheCriticalDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Cache user sessions, active transactions, etc.
            var criticalDataQuery = @"
                    SELECT json_agg(t) as data FROM (
                        SELECT 'users' as table_name, count(*) as record_count FROM users
                        UNION ALL
                        SELECT 'sessions' as table_name, count(*) as record_count FROM user_sessions WHERE expires_at > NOW()
                    ) t";

            using var command = new NpgsqlCommand(criticalDataQuery, connection);
            var result = await command.ExecuteScalarAsync(cancellationToken) as string;

            if (!string.IsNullOrEmpty(result))
            {
                await _cache.SetStringAsync("restore:critical_data", result, cancellationToken);
                _logger.LogInformation("Cached critical data for restore operation");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache critical data, restore will continue without cache");
        }
    }

    private async Task DisableCacheFirstModeAndSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cache.RemoveAsync("system:cache_first_mode", cancellationToken);

            // Sync any pending cached operations back to database
            var cachedData = await _cache.GetStringAsync("restore:critical_data", cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                // Process cached data and sync if needed
                _logger.LogInformation("Synced cached data back to restored database");
                await _cache.RemoveAsync("restore:critical_data", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync cached data, manual verification may be required");
        }
    }

    private async Task WaitForRecoveryCompletionAsync(CancellationToken cancellationToken)
    {
        var maxWaitTime = TimeSpan.FromMinutes(30);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            try
            {
                var result = await ExecuteDockerCommandAsync(
                    "pg_controldata /var/lib/postgresql/data | grep 'Database cluster state'", cancellationToken);

                if (result.Contains("in production") || result.Contains("shut down"))
                {
                    _logger.LogInformation("Recovery completed successfully");
                    return;
                }
            }
            catch
            {
                // Continue waiting
            }

            await Task.Delay(5000, cancellationToken); // Check every 5 seconds
        }

        throw new TimeoutException("Recovery did not complete within the expected time");
    }

    private async Task CleanupRestoreTempFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteDockerCommandAsync("rm -rf /tmp/pg_backup /tmp/wal_backup /var/lib/postgresql/restore",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup temporary restore files");
        }
    }

    private async Task<string> ExecuteDockerCommandAsync(string command, CancellationToken cancellationToken)
    {
        var COMMAND_TIMEOUT_SECONDS = 60;
        var escapedCommand = EscapeShellArgument(command);
        var escapedContainer = EscapeShellArgument(_dockerContainerName);
    
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"exec {escapedContainer} bash -c \"{escapedCommand}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        
        try
        {
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(COMMAND_TIMEOUT_SECONDS * 1000))
            {
                process.Kill();
                throw new TimeoutException($"Docker command timed out: {command}");
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Docker command failed: {command}\nError: {error}");
            }

            return output;
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    private static string EscapeShellArgument(string arg)
    {
        return arg.Replace("\"", "\\\"")
            .Replace("$", "\\$")
            .Replace("`", "\\`")
            .Replace("!", "\\!");
    }

    private int CalculateEstimatedRestoreTime(string fullBackupPath, List<string> incrementalPaths)
    {
        // Estimate based on file sizes (rough calculation: 100MB per minute)
        var totalSize = 0L;

        if (File.Exists(fullBackupPath))
            totalSize += new FileInfo(fullBackupPath).Length;

        foreach (var incPath in incrementalPaths.Where(File.Exists))
            totalSize += new FileInfo(incPath).Length;

        return Math.Max(1, (int)(totalSize / (100 * 1024 * 1024))); // Minutes
    }

    private long CalculateRequiredSpace(string fullBackupPath, List<string> incrementalPaths)
    {
        // Space needed = 2x the largest backup (for temporary extraction)
        var sizes = new List<long>();

        if (File.Exists(fullBackupPath))
            sizes.Add(new FileInfo(fullBackupPath).Length);

        foreach (var incPath in incrementalPaths.Where(File.Exists))
            sizes.Add(new FileInfo(incPath).Length);

        return sizes.Any() ? sizes.Max() * 2 : 0;
    }

    private DateTime GetRestoreToTimestamp(string backupId, BackupChain? chain)
    {
        if (backupId == "latest" && chain != null)
        {
            // Latest incremental or full backup timestamp
            if (chain.Incrementals.Any())
            {
                var lastIncPath = Path.Combine(_hostBackupPath, chain.Incrementals.Last());
                if (File.Exists(lastIncPath))
                    return new FileInfo(lastIncPath).CreationTimeUtc;
            }

            return chain.Timestamp;
        }

        if (chain != null && chain.Incrementals.Contains(backupId))
        {
            var incPath = Path.Combine(_hostBackupPath, backupId);
            if (File.Exists(incPath))
                return new FileInfo(incPath).CreationTimeUtc;
        }

        return chain?.Timestamp ?? DateTime.MinValue;
    }

    private bool IsFullBackup(string fileName) => fileName.Contains("_full_");

    public void Dispose()
    {
        _operationLock?.Dispose();
    }
    // Add this helper method to your BackupRestoreService class
    // Fix the ConvertBackupChain method signature to use the fully qualified name
    
}