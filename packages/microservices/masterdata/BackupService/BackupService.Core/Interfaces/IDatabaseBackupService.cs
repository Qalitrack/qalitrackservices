using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;

namespace BackupService.Core.Interfaces;

public interface IDatabaseBackupService
{
    /// <summary>
    /// Creates a SQL dump backup (only BackupType.Full supported)
    /// </summary>
    Task<BackupResult> CreateBackupAsync(BackupType type, string microservice, string? cronSchedule = null, CancellationToken ct = default);
    
    /// <summary>
    /// Restores a SQL dump backup from the specified file path
    /// </summary>
    Task<RestoreResult> RestoreBackupAsync(string microservice, string backupFilePath, CancellationToken ct = default);
    
 
    /// <summary>
    /// Gets list of available backup files
    /// </summary>
    Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? microservice = null, CancellationToken ct = default);
    
    
    
    /// <summary>
    /// Gets backup statistics
    /// </summary>
    Task<BackupStatistics> GetBackupStatisticsAsync(string? microservice = null, CancellationToken ct = default);
    
    /// <summary>
    /// Unschedules a backup (only BackupType.Full supported)
    /// </summary>
    Task<bool> UnscheduleBackupAsync(string microservice, BackupType backupType, CancellationToken ct);
    
    /// <summary>
    /// Gets list of scheduled backups
    /// </summary>
    Task<List<ScheduledBackup>> GetScheduledBackupsAsync(CancellationToken ct);
}