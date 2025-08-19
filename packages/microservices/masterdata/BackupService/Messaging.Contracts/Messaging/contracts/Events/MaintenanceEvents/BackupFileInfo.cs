namespace Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

public record BackupFileInfo
{
    public string BackupId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string BackupType { get; set; } = string.Empty; // Full or Incremental
    public DateTime CreatedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public string ChainId { get; set; } = string.Empty;
    public bool IsLatest { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    
}
