using BackupService.Core.Enums;

namespace BackupService.Core.Dtos;

public class BackupFileInfo
{
    public string BackupId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public BackupType BackupType { get; init; } = BackupType.Full;
    public DateTime CreatedAt { get; init; }
    public long FileSizeBytes { get; init; }
    public string ChainId { get; init; } = string.Empty;
    public bool IsLatest { get; init; }
    public string ServiceName { get; init; } = string.Empty;

}