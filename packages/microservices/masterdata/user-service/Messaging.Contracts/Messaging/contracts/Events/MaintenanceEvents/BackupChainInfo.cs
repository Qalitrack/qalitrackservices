namespace Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

public record BackupChainInfo
{
    public string ChainId { get; set; } = string.Empty;
    public string FullBackupFile { get; set; } = string.Empty;
    public DateTime FullBackupTimestamp { get; set; }
    public List<string> IncrementalFiles { get; set; } = new();
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public bool IsValid { get; set; }
    public bool IsLatest { get; set; }
    public string ServiceName { get; set; } = string.Empty; // Which service owns this chain
}