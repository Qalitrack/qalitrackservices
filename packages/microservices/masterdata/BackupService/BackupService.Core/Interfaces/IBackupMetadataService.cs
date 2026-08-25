using BackupService.Core.Dtos;
using BackupService.Core.Entities;

namespace BackupService.Core.Interfaces;

public interface IBackupMetadataService : IDisposable
{
   
    Task UpdateMetadataWithFullBackupAsync(BackupResult backupResult, string microservice, CancellationToken ct = default);

    /// <summary>
    /// Appends a pgBackRest incremental backup label to the most recent physical chain
    /// for the given microservice, updating its tracked LSN.
    /// </summary>
    Task UpdateMetadataWithIncrementalBackupAsync(BackupResult backupResult, string microservice, CancellationToken ct = default);

    Task<List<BackupChain>> GetAllBackupChainsAsync(string? microservice = null, CancellationToken ct = default);
    Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? microservice = null, CancellationToken ct = default);
  
}