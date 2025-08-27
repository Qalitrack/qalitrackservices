using System.Threading;
using System.Threading.Tasks;
using BackupService.Core.Dtos;

namespace BackupService.Core.Interfaces;

public interface IBackupRestoreService
{
    /// <summary>
    /// Restores a SQL dump backup to a PostgreSQL database
    /// </summary>
    /// <param name="microservice">Name of the microservice</param>
    /// <param name="backupFilePath">Full path to the SQL dump backup file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>RestoreResult with details of the restore operation</returns>
    Task<RestoreResult> RestoreBackupAsync(string microservice, string backupFilePath, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Preview what would happen during a restore operation
    /// </summary>
    /// <param name="microservice">Name of the microservice</param>
    /// <param name="backupFilePath">Full path to the SQL dump backup file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>RestorePreviewResult with estimated restore details</returns>
    Task<RestorePreviewResult> PreviewRestoreAsync(string microservice, string backupFilePath, 
        CancellationToken ct = default);
}