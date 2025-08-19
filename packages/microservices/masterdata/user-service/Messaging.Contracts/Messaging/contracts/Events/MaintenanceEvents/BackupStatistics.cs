namespace Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

public record  BackupStatistics
{
    public DateTime GeneratedAt { get; set; }
    public int TotalChains { get; set; }
    public int TotalBackupFiles { get; set; }
    public int TotalFullBackups { get; set; }
    public int TotalIncrementalBackups { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime? OldestBackup { get; set; }
    public DateTime? NewestBackup { get; set; }
    public int ValidChains { get; set; }
    public int InvalidChains { get; set; }
}