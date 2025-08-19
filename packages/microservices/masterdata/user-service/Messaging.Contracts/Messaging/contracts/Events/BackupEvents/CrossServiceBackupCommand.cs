using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

namespace Messaging.Contracts.Messaging.contracts.Events.BackupEvents;

public record CrossServiceBackupCommand
{
    
    public string CommandId { get; set; } = string.Empty;
    public BackupCommandType CommandType { get; set; }
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
    public List<BackupFileInfo> BackupFiles { get; set; } = new();
    public string? ChainId { get; set; }
    public List<BackupChainInfo>? ChainInfo { get; set; }
    public bool IsCritical { get; set; } = false;
}
