using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class SyncConfiguration : BaseEntity
{
    public string ConfigId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceSiteId { get; set; } = string.Empty;
    public string TargetSiteId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public SyncDirection Direction { get; set; }
    public SyncMode Mode { get; set; }
    public SyncPriority Priority { get; set; }
    public ConflictResolutionStrategy DefaultConflictResolution { get; set; }
    public int SyncIntervalMinutes { get; set; }
    public bool IsEnabled { get; set; }
    public string? FilterCondition { get; set; }
    public string? FieldMapping { get; set; }
    public string? TransformationRules { get; set; }
    public DateTime? LastSync { get; set; }
    public DateTime? NextSync { get; set; }
    public string? ConfigurationJson { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string? NotificationEmails { get; set; }
    public bool EnableNotifications { get; set; }
}