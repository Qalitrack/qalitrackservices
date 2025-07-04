using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class OperationalAlert : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string AlertId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [MaxLength(100)]
    public string? WeighbridgeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public required string Message { get; set; }
    
    [Required]
    public AlertType Type { get; set; }
    
    [Required]
    public AlertSeverity Severity { get; set; }
    
    [Required]
    public AlertStatus Status { get; set; } = AlertStatus.Active;
    
    [Required]
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? AcknowledgedAt { get; set; }
    
    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string? ResolvedBy { get; set; }
    
    [MaxLength(1000)]
    public string? Resolution { get; set; }
    
    [Required]
    public AlertSource Source { get; set; }
    
    [MaxLength(100)]
    public string? SourceId { get; set; }
    
    public List<string> Recipients { get; set; } = new();
    
    public List<AlertAction> Actions { get; set; } = new();
    
    public List<AlertNotification> Notifications { get; set; } = new();
    
    public AlertRule? Rule { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public List<string> Tags { get; set; } = new();
    
    public int EscalationLevel { get; set; } = 0;
    
    public DateTime? NextEscalationTime { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    public int RecurrenceCount { get; set; } = 0;
    
    public DateTime? LastOccurrence { get; set; }
    
    public TimeSpan? SuppressUntil { get; set; }
    
    public bool IsSupressed { get; set; } = false;
}

public class AlertAction
{
    [Required]
    [MaxLength(100)]
    public required string ActionId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public ActionType Type { get; set; }
    
    [Required]
    public ActionStatus Status { get; set; } = ActionStatus.Pending;
    
    public DateTime? ExecutedAt { get; set; }
    
    [MaxLength(100)]
    public string? ExecutedBy { get; set; }
    
    [MaxLength(1000)]
    public string? Result { get; set; }
    
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public int Priority { get; set; } = 0;
    
    public bool IsAutomated { get; set; } = false;
    
    public TimeSpan? ExecutionDelay { get; set; }
}

public class AlertNotification
{
    [Required]
    [MaxLength(100)]
    public required string NotificationId { get; set; }
    
    [Required]
    public NotificationType Type { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Recipient { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Subject { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public required string Body { get; set; }
    
    [Required]
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeliveredAt { get; set; }
    
    [MaxLength(500)]
    public string? DeliveryStatus { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public DateTime? NextRetryAt { get; set; }
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}

public class AlertRule
{
    [Required]
    [MaxLength(100)]
    public required string RuleId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public required string Condition { get; set; }
    
    [Required]
    public AlertType TriggerType { get; set; }
    
    [Required]
    public AlertSeverity Severity { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public TimeSpan? CooldownPeriod { get; set; }
    
    public int? MaxOccurrences { get; set; }
    
    public TimeSpan? SuppressDuration { get; set; }
    
    public List<string> NotificationChannels { get; set; } = new();
    
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum AlertType
{
    Capacity,
    Performance,
    Maintenance,
    Quality,
    Safety,
    Security,
    System,
    Operational,
    Compliance,
    Custom
}

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical,
    Emergency
}

public enum AlertStatus
{
    Active,
    Acknowledged,
    Resolved,
    Suppressed,
    Expired,
    Cancelled
}

public enum AlertSource
{
    System,
    Sensor,
    User,
    External,
    Automated,
    Manual
}

public enum ActionType
{
    Notification,
    Escalation,
    Automation,
    Logging,
    Reporting,
    Integration,
    Custom
}

public enum ActionStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    Skipped
}

public enum NotificationType
{
    Email,
    SMS,
    Push,
    Webhook,
    Slack,
    Teams,
    Custom
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Cancelled
}