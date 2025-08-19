using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

namespace Messaging.Contracts.Messaging.contracts;

public interface IBackupVerificationService
{
    /// <summary>
    /// Verifies the integrity of a backup file
    /// </summary>
    Task VerifyBackupIntegrityAsync(string backupPath, BackupType backupType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of the database after restore
    /// </summary>
    Task VerifyDatabaseIntegrityAsync(string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of an entire backup chain
    /// </summary>
    Task<bool> VerifyBackupChainIntegrityAsync(string fullBackupPath, List<string> incrementalPaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a comprehensive health report for multiple backups
    /// </summary>
    Task<BackupHealthReport> GenerateBackupHealthReportAsync(List<string> backupPaths, CancellationToken cancellationToken = default);
}
