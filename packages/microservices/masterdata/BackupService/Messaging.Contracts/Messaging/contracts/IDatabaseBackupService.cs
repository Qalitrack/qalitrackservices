using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
using Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;

namespace Messaging.Contracts.Messaging.contracts;

public interface IDatabaseBackupService
{
// Backup Creation
    Task<BackupResult> CreateBackupAsync(BackupType backupType, CancellationToken cancellationToken = default);

    // Backup Restoration
    Task<RestoreResult> RestoreBackupAsync(string backupId = "latest", CancellationToken cancellationToken = default);
    Task<RestorePreviewResult> PreviewRestoreAsync(string backupId);

    // Backup Management
    Task<List<BackupChainInfo>> GetAllBackupChainsAsync();
    Task<List<BackupFileInfo>> GetAvailableBackupsAsync();
    Task<BackupChainInfo?> GetBackupChainInfoAsync(string identifier);

    // Health and Verification
    Task<BackupHealthReport> GenerateHealthReportAsync(CancellationToken cancellationToken = default);
    Task ValidateAllBackupsAsync(CancellationToken cancellationToken = default);
}

