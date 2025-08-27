using BackupService.Core.Enums;

namespace BackupService.Core.Dtos;

public class BackupHealthResult
{
    public string BackupPath { get; init; } = string.Empty;
    public string BackupName { get; init; } = string.Empty;
    public BackupType BackupType { get; init; }
    public bool IsHealthy { get; set; }
    public string HealthMessage { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public long FileSizeBytes { get; init; }
    public DateTime LastModified { get; init; }
}