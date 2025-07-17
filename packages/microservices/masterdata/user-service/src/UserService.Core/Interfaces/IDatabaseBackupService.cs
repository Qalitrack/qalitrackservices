using System.Threading;
using System.Threading.Tasks;
using UserService.Core.Enums;

namespace UserService.Core.Interfaces
{
    public interface IDatabaseBackupService
    {
        Task<string> CreateBackupAsync(BackupType backupType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifies the integrity of a backup file
        /// </summary>
        /// <param name="backupPath">Path to the backup file to verify</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that completes when verification is done</returns>
        /// <exception cref="FileNotFoundException">If backup file doesn't exist</exception>
        /// <exception cref="InvalidDataException">If backup file is corrupted or invalid</exception>
        Task VerifyBackupIntegrityAsync(string backupPath, CancellationToken cancellationToken = default);
    }
}