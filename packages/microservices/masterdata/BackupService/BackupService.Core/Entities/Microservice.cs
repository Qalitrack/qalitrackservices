using BackupService.Core.Enums;

namespace BackupService.Core.Entities;

public class Microservice
{
    
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public MicroserviceStatus Status { get; set; } = MicroserviceStatus.Active;
    public DateTime? LastBackupAt { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

}