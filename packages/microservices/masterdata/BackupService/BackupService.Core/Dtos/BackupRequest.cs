using BackupService.Core.Enums;

namespace BackupService.Core.Dtos;

public class BackupRequest
{
    
    public string Microservice { get; init; } = string.Empty;
    public BackupType Type { get; init; }
    public string SaveLocation { get; init; } = string.Empty;
    public string? CronSchedule { get; init; }
}