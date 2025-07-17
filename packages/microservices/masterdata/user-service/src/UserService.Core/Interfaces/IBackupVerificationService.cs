using System.Threading;
using System.Threading.Tasks;

namespace UserService.Core.Interfaces;

public interface IBackupVerificationService
{
    /// <summary>
    /// Verifies all backups in the configured backup directory
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when verification is done</returns>
    Task VerifyAllBackupsAsync(CancellationToken cancellationToken = default);
}
