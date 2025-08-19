using Messaging.Contracts.Messaging.contracts.Enums;

namespace Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

public record BackupHealthResult
{
    public string BackupPath { get; set; } = string.Empty;
    public string BackupName { get; set; } = string.Empty;
    public BackupType BackupType { get; set; }
    public bool IsHealthy { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string HealthMessage { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
}