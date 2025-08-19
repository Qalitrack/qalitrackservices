namespace Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;

public record RestorePreviewResult{ 
    public string BackupId { get; set; } = string.Empty;
    public string ChainId { get; set; } = string.Empty;
    public string FullBackupFile { get; set; } = string.Empty;
    public List<string> IncrementalFiles { get; set; } = new();
    public int TotalFilesToProcess { get; set; }
    public int EstimatedRestoreTimeMinutes { get; set; }
    public long RequiredSpaceBytes { get; set; }
    public DateTime RestoreToTimestamp { get; set; }
    public string ServiceName { get; set; } = string.Empty;
}