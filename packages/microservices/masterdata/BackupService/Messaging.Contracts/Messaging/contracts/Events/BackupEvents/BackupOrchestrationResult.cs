using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;

namespace Messaging.Contracts.Messaging.contracts.Events.BackupEvents;

public record BackupOrchestrationResult
{
    public string OrchestrationId { get; set; } = Guid.NewGuid().ToString();
    public BackupCommandType CommandType { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public Dictionary<string, CrossServiceBackupResponse> ServiceResponses { get; set; } = new();   
    public string Message { get; set; } = string.Empty;     

};