using BackupService.Core.Enums;

namespace BackupService.Core.Dtos;

public class MicroserviceRequest
{
    public string Name { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
    public MicroserviceStatus Status { get; init; } = MicroserviceStatus.Active;
    
    public DateTime? LastBackupAt { get; init; }
    
}