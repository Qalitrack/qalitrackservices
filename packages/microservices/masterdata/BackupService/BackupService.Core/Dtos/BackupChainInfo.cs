namespace BackupService.Core.Dtos;

public class BackupChainInfo
{
    
    public string ChainId { get; init; } = string.Empty;
    public string FullBackupFile { get; init; } = string.Empty;
    public DateTime FullBackupTimestamp { get; init; }
    public List<string> IncrementalFiles { get; init; } = new();
    public int TotalFiles { get; init; }
    public long TotalSizeBytes { get; init; }
    public bool IsValid { get; init; }
    public bool IsLatest { get; init; }
    public string ServiceName { get; init; } = string.Empty;
}