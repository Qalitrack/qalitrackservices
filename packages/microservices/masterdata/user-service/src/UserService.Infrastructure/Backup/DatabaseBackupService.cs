using System.Diagnostics;
using Messaging.Contracts.Messaging;
using Microsoft.Extensions.Logging;
using Messaging.Contracts.Messaging.contracts;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
using Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;

namespace UserService.Infrastructure.Backup;

public class DatabaseBackupService : IDatabaseBackupService, IAsyncDisposable
{
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly IBackupCreationService _creationService;
    private readonly IBackupRestoreService _restoreService;
    private readonly IBackupMetadataService _metadataService;
    private readonly IBackupVerificationService _verificationService;
    private bool _disposed;

    public DatabaseBackupService(
        ILogger<DatabaseBackupService> logger,
        IBackupCreationService creationService,
        IBackupRestoreService restoreService,
        IBackupMetadataService metadataService,
        IBackupVerificationService verificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _creationService = creationService ?? throw new ArgumentNullException(nameof(creationService));
        _restoreService = restoreService ?? throw new ArgumentNullException(nameof(restoreService));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
    }

    #region Backup Operations

    public async Task<BackupResult> CreateBackupAsync(BackupType backupType, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(CreateBackupAsync), backupType.ToString());
            var stopwatch = Stopwatch.StartNew();
            
            var result = await _creationService.CreateBackupAsync(backupType, cancellationToken);
            
            LogMethodSuccess(stopwatch.ElapsedMilliseconds);
            return result;
        }, "Failed to create backup");
    }

    #endregion

    #region Restore Operations

    public async Task<RestoreResult> RestoreBackupAsync(string backupId = "latest", CancellationToken cancellationToken = default)
    {
        ValidateBackupId(backupId);
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(RestoreBackupAsync), backupId);
            var stopwatch = Stopwatch.StartNew();
            
            var result = await _restoreService.RestoreBackupAsync(backupId, cancellationToken);
            
            LogMethodSuccess(stopwatch.ElapsedMilliseconds);
            return result;
        }, "Failed to restore backup");
    }

    public async Task<RestorePreviewResult> PreviewRestoreAsync(string backupId)
    {
        ValidateBackupId(backupId);
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(PreviewRestoreAsync), backupId);
            return await _restoreService.PreviewRestoreAsync(backupId);
        }, "Failed to preview restore");
    }

    #endregion

    #region Backup Management

    public async Task<List<BackupChainInfo>> GetAllBackupChainsAsync()
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(GetAllBackupChainsAsync));
            return await _metadataService.GetAllBackupChainsAsync();
        }, "Failed to get backup chains");
    }

    public async Task<List<BackupFileInfo>> GetAvailableBackupsAsync()
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(GetAvailableBackupsAsync));
            return await _metadataService.GetAvailableBackupsAsync();
        }, "Failed to get available backups");
    }

    public async Task<BackupChainInfo?> GetBackupChainInfoAsync(string identifier)
    {
        ValidateBackupId(identifier);
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(GetBackupChainInfoAsync), identifier);
            return await _metadataService.GetBackupChainInfoAsync(identifier);
        }, "Failed to get backup chain info");
    }

    #endregion

    #region Health and Verification

    public async Task<BackupHealthReport> GenerateHealthReportAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(GenerateHealthReportAsync));
            var stopwatch = Stopwatch.StartNew();
            
            var backupFiles = await _metadataService.GetAvailableBackupsAsync();
            var healthReport = await _verificationService.GenerateBackupHealthReportAsync(
                backupFiles.Select(f => f.FileName).ToList(), 
                cancellationToken);
            
            LogMethodSuccess(stopwatch.ElapsedMilliseconds);
            return healthReport;
        }, "Failed to generate health report");
    }

    public async Task ValidateAllBackupsAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(ValidateAllBackupsAsync));
            var stopwatch = Stopwatch.StartNew();
            
            await _metadataService.ValidateMetadataIntegrityAsync();
            
            LogMethodSuccess(stopwatch.ElapsedMilliseconds);
        }, "Failed to validate backups");
    }

    #endregion

    #region Cleanup Operations
    

    public async Task<BackupStatistics> GetBackupStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            using var _ = LogMethodStart(nameof(GetBackupStatisticsAsync));
            
            var chains = await _metadataService.GetAllBackupChainsAsync();
            var backupFiles = await _metadataService.GetAvailableBackupsAsync();
            
            return new BackupStatistics
            {
                GeneratedAt = DateTime.UtcNow,
                TotalChains = chains.Count,
                TotalBackupFiles = backupFiles.Count,
                TotalFullBackups = backupFiles.Count(b => b.BackupType.Equals(BackupType.Full)),
                TotalIncrementalBackups = backupFiles.Count(b => b.BackupType.Equals(BackupType.Incremental)),
                TotalSizeBytes = chains.Sum(c => c.TotalSizeBytes),
                OldestBackup = backupFiles.MinBy(b => b.CreatedAt)?.CreatedAt ?? DateTime.MinValue,
                NewestBackup = backupFiles.MaxBy(b => b.CreatedAt)?.CreatedAt ?? DateTime.MinValue,
                ValidChains = chains.Count(c => c.IsValid),
                InvalidChains = chains.Count(c => !c.IsValid)
            };
        }, "Failed to get backup statistics");
    }

    #endregion

    #region Helper Methods

    private IDisposable LogMethodStart(string methodName, string? parameter = null)
    {
        _logger.LogDebug("Starting {MethodName}{Parameter}", 
            methodName, 
            parameter != null ? $" with parameter: {parameter}" : "");
        return new DisposableAction(() => _logger.LogDebug("Completed {MethodName}", methodName));
    }

    private void LogMethodSuccess(long elapsedMs)
    {
        _logger.LogDebug("Operation completed successfully in {ElapsedMs}ms", elapsedMs);
    }

    private async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> operation, string errorMessage)
    {
        try
        {
            return await operation();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            throw;
        }
    }

    private async Task ExecuteWithErrorHandling(Func<Task> operation, string errorMessage)
    {
        try
        {
            await operation();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            throw;
        }
    }

    private void ValidateBackupId(string backupId)
    {
        if (string.IsNullOrWhiteSpace(backupId))
        {
            throw new ArgumentException("Backup ID cannot be empty", nameof(backupId));
        }
    }

    private class DisposableAction : IDisposable
    {
        private readonly Action _onDispose;
        public DisposableAction(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;
        
        (_creationService as IDisposable)?.Dispose();
        (_restoreService as IDisposable)?.Dispose();
        (_metadataService as IDisposable)?.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_creationService is IAsyncDisposable creationDisposable)
            await creationDisposable.DisposeAsync().ConfigureAwait(false);

        if (_restoreService is IAsyncDisposable restoreDisposable)
            await restoreDisposable.DisposeAsync().ConfigureAwait(false);

        if (_metadataService is IAsyncDisposable metadataDisposable)
            await metadataDisposable.DisposeAsync().ConfigureAwait(false);
    }

    #endregion
}