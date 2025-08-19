using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
using Messaging.Contracts.Messaging.contracts.Events.RestoreEvents;

namespace Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

public record CrossServiceBackupResponse
{
    public string CommandId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public BackupResult? BackupResult { get; set; }
    public RestoreResult? RestoreResult { get; set; }
    public List<BackupChainInfo>? ChainInfo { get; set; }
    public RestorePreviewResult? RestorePreviewResult { get; set; } // Add this
    public BackupHealthReport? HealthReport { get; set; } // Add this
}
