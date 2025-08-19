using Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;

namespace Messaging.Contracts.Messaging.contracts;

public interface IBackupRestoreService : IDisposable
{
    /// <summary>
    /// Restores a backup (either full or incremental chain)
    /// </summary>
    /// <param name="backupId">ID of the backup to restore ("latest" for most recent)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the restore operation</returns>
    Task<RestoreResult> RestoreBackupAsync(string backupId = "latest", CancellationToken cancellationToken = default);

    /// <summary>
    /// Provides a preview of what would be restored for a given backup ID
    /// </summary>
    /// <param name="backupId">ID of the backup to preview ("latest" for most recent)</param>
    /// <returns>Preview information about the restore operation</returns>
    Task<RestorePreviewResult> PreviewRestoreAsync(string backupId);
}
