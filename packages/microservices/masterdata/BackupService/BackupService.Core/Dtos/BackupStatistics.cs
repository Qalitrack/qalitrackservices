namespace BackupService.Core.Dtos;

public class BackupStatistics
{
    public DateTime GeneratedAt { get; init; }
    public int TotalChains { get; init; }
    public int TotalBackupFiles { get; init; }
    public int TotalFullBackups { get; init; }
    public int TotalIncrementalBackups { get; init; }
    public long TotalSizeBytes { get; init; }
    public DateTime OldestBackup { get; init; }
    public DateTime NewestBackup { get; init; }
    public int ValidChains { get; init; }
    public int InvalidChains { get; init; }
}