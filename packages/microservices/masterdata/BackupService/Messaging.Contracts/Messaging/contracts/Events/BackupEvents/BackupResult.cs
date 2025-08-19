using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

namespace Messaging.Contracts.Messaging.contracts.Events.BackupEvents;


public record BackupResult
{
    public string BackupId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public BackupType BackupType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsValid { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ChainId { get; set; } = string.Empty;
    public BackupChainInfo? ChainInfo { get; set; }
}