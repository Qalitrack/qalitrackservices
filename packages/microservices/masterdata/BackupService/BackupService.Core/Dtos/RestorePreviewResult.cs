namespace BackupService.Core.Dtos;

public class RestorePreviewResult
{
    public string BackupId { get; init; } = string.Empty;
    public string? ChainId { get; init; }
    public string? FullBackupFile { get; init; }
    public List<string> IncrementalFiles { get; init; } = new();
    public int TotalFilesToProcess { get; init; }
    public int EstimatedRestoreTimeMinutes { get; init; }
    public long RequiredSpaceBytes { get; init; }
    public DateTime RestoreToTimestamp { get; init; }
    public string ServiceName { get; init; } = string.Empty;
}