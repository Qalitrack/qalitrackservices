namespace BackupService.Core.Dtos;

public class RestoreResult
{
    public string BackupId { get; init; } = string.Empty;
    public string ServiceName { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool IsSuccessful { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorDetails { get; init; }
    public string? FullBackupUsed { get; init; }
    public List<string> IncrementalBackupsUsed { get; init; } = new();
    public int TotalFilesProcessed { get; init; }

}