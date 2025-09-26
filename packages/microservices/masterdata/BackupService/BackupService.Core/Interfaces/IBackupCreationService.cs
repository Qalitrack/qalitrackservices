using BackupService.Core.Dtos;
using BackupService.Core.Enums;

namespace BackupService.Core.Interfaces;

public interface IBackupCreationService
{
    /// <summary>
    /// Creates a backup for the specified microservice. The connection string will be retrieved from the database.
    /// </summary>
    /// <param name="backupType">Type of backup to create (Full or Incremental)</param>
    /// <param name="microservice">Name of the microservice to backup</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>BackupResult containing details about the created backup</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified microservice is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when the microservice is not in Active status or backup fails</exception>
    /// <exception cref="TimeoutException">Thrown when the backup operation times out</exception>
    Task<BackupResult> CreateBackupAsync(BackupType backupType, string microservice, CancellationToken ct = default);
}