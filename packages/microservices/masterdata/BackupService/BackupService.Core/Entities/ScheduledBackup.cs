using BackupService.Core.Enums;

namespace BackupService.Core.Entities;

// Place this in a suitable namespace, e.g., BackupService.Core.Dtos

public class ScheduledBackup
{
    public string Microservice { get; set; } = string.Empty;
    public BackupType BackupType { get; set; }
    public string? CronSchedule { get; set; } = string.Empty;
    public DateTime? NextFireTime { get; set; }
    public string SaveLocation { get; set; } = string.Empty;
}