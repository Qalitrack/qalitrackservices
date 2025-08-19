using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;

namespace Messaging.Contracts.Messaging.contracts;

public interface IBackupCreationService : IDisposable
{
    /// <summary>
    /// Creates a new backup of the specified type
    /// </summary>
    Task<BackupResult> CreateBackupAsync(BackupType backupType, CancellationToken cancellationToken = default);
}