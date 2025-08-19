using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

namespace Messaging.Contracts.Messaging.contracts;

 public interface IBackupMetadataService : IDisposable
    {
        /// <summary>
        /// Loads the backup metadata from storage
        /// </summary>
        Task<BackupMetadata> LoadMetadataAsync();

        /// <summary>
        /// Saves the backup metadata to storage
        /// </summary>
        Task SaveMetadataAsync(BackupMetadata metadata);

        /// <summary>
        /// Updates metadata when a new full backup is created
        /// </summary>
        Task UpdateMetadataWithFullBackupAsync(string backupFileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates metadata when a new incremental backup is created
        /// </summary>
        Task UpdateMetadataWithIncrementalBackupAsync(string backupFileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the most recent full backup chain
        /// </summary>
        Task<BackupChain?> GetLatestFullBackupChainAsync();

        /// <summary>
        /// Gets information about all backup chains
        /// </summary>
        Task<List<BackupChainInfo>> GetAllBackupChainsAsync();

        /// <summary>
        /// Gets information about a specific backup chain
        /// </summary>
        Task<BackupChainInfo?> GetBackupChainInfoAsync(string identifier);

        /// <summary>
        /// Gets a list of all available backup files
        /// </summary>
        Task<List<BackupFileInfo>> GetAvailableBackupsAsync();

        /// <summary>
        /// Gets a backup chain by backup ID or identifier
        /// </summary>
        Task<BackupChain?> GetBackupChainAsync(string backupId);

        /// <summary>
        /// Finds the chain that contains a specific incremental backup
        /// </summary>
        Task<BackupChain?> FindChainForIncrementalAsync(string incrementalFile);

        /// <summary>
        /// Removes a backup chain from metadata
        /// </summary>
        Task RemoveBackupChainAsync(string chainId);

        /// <summary>
        /// Validates metadata integrity and removes references to missing files
        /// </summary>
        Task ValidateMetadataIntegrityAsync();

        /// <summary>
        /// Gets a consistent chain ID for a backup chain
        /// </summary>
        string GetChainId(BackupChain chain);

        /// <summary>
        /// Maps a backup chain to chain info with file system validation
        /// </summary>
        BackupChainInfo MapToBackupChainInfo(BackupChain chain);
    }
