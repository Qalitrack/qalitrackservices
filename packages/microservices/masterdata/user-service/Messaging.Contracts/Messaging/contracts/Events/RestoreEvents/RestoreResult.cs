namespace Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;

public record RestoreResult {
    public string BackupId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public string FullBackupUsed { get; set; } = string.Empty;
    public List<string> IncrementalBackupsUsed { get; set; } = new();
    public int TotalFilesProcessed { get; set; }
}
