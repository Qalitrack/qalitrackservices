using BackupService.Core.Enums;
namespace BackupService.Core.Dtos;

public class BackupResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string BackupId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public BackupType BackupType { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? FilePath { get; init; }
    public long FileSizeBytes { get; init; }
    public bool IsValid { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? ChainId { get; init; }
    public BackupChainInfo? ChainInfo { get; init; }
    public string? Lsn { get; init; }
    public int Timeline { get; init; }
}