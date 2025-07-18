using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ReportService.Core.Entities;

public class ScheduleExecution : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ScheduleId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    // Execution Details
    public DateTime ScheduledTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public ScheduleExecutionStatus Status { get; set; } = ScheduleExecutionStatus.NotStarted;
    
    public TimeSpan? Duration
    {
        get => StartTime.HasValue && EndTime.HasValue ? EndTime.Value - StartTime.Value : null;
    }
    
    // Execution Context
    [StringLength(50)]
    public string? ExecutionId { get; set; } // Unique execution identifier
    
    [StringLength(50)]
    public string? TriggeredBy { get; set; } // User, System, or External
    
    [StringLength(100)]
    public string? TriggerSource { get; set; } // Manual, Scheduled, API, etc.
    
    [StringLength(50)]
    public string? ExecutionNode { get; set; } // Which server/node executed this
    
    // Results and Output
    [StringLength(1000)]
    public string? ResultSummary { get; set; }
    
    [StringLength(5000)]
    public string? OutputLog { get; set; }
    
    [StringLength(5000)]
    public string? ErrorLog { get; set; }
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? ErrorCode { get; set; }
    
    public int? ExitCode { get; set; }
    
    // Resource Usage
    public long? MemoryUsedBytes { get; set; }
    public decimal? CpuUsagePercentage { get; set; }
    public long? DiskUsedBytes { get; set; }
    public long? NetworkBytesTransferred { get; set; }
    
    // Progress Tracking
    public decimal? ProgressPercentage { get; set; } = 0;
    
    [StringLength(500)]
    public string? ProgressMessage { get; set; }
    
    public int? ItemsProcessed { get; set; } = 0;
    public int? TotalItems { get; set; }
    
    // Retry Information
    public int AttemptNumber { get; set; } = 1;
    public int MaxAttempts { get; set; } = 1;
    
    [StringLength(50)]
    public string? PreviousExecutionId { get; set; } // For retries
    
    public bool IsRetry { get; set; } = false;
    
    [StringLength(1000)]
    public string? RetryReason { get; set; }
    
    // Dependencies
    public bool WaitedForDependencies { get; set; } = false;
    public TimeSpan? DependencyWaitTime { get; set; }
    
    [StringLength(1000)]
    public string? DependencyStatus { get; set; } // JSON object
    
    // Notifications
    public bool NotificationsSent { get; set; } = false;
    
    [StringLength(1000)]
    public string? NotificationRecipients { get; set; } // JSON array
    
    public DateTime? NotificationSentDate { get; set; }
    
    // Performance Metrics
    public TimeSpan? QueueTime { get; set; } // Time spent waiting in queue
    public TimeSpan? InitializationTime { get; set; }
    public TimeSpan? ProcessingTime { get; set; }
    public TimeSpan? FinalizationTime { get; set; }
    
    // Quality Metrics
    public decimal? DataQualityScore { get; set; }
    public int? ValidationErrors { get; set; } = 0;
    public int? WarningCount { get; set; } = 0;
    
    [StringLength(1000)]
    public string? QualityIssues { get; set; } // JSON array
    
    // Business Metrics
    [StringLength(1000)]
    public string? BusinessMetrics { get; set; } // JSON object for custom metrics
    
    public decimal? BusinessValue { get; set; }
    
    [StringLength(100)]
    public string? BusinessCategory { get; set; }
    
    // Audit Trail
    [StringLength(1000)]
    public string? AuditTrail { get; set; } // JSON array of audit events
    
    public bool RequiresReview { get; set; } = false;
    
    [StringLength(50)]
    public string? ReviewedBy { get; set; }
    
    public DateTime? ReviewDate { get; set; }
    
    [StringLength(1000)]
    public string? ReviewComments { get; set; }
    
    // Environment Information
    [StringLength(100)]
    public string? EnvironmentName { get; set; } // dev, test, prod
    
    [StringLength(50)]
    public string? ApplicationVersion { get; set; }
    
    [StringLength(1000)]
    public string? SystemConfiguration { get; set; } // JSON object
    
    // Related Entities
    [StringLength(1000)]
    public string? GeneratedReportIds { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? ProcessedFileIds { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? CreatedExportIds { get; set; } // JSON array
    
    // Alerts and Monitoring
    public bool TriggeredAlerts { get; set; } = false;
    
    [StringLength(1000)]
    public string? AlertsTriggered { get; set; } // JSON array
    
    public bool RequiresEscalation { get; set; } = false;
    
    [StringLength(50)]
    public string? EscalatedTo { get; set; }
    
    public DateTime? EscalationDate { get; set; }
    
    // JSON properties for flexible data storage
    public string? ExecutionContextJson { get; set; }
    public string? MetricsJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? ExecutionContext
    {
        get => string.IsNullOrEmpty(ExecutionContextJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ExecutionContextJson);
        set => ExecutionContextJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metrics
    {
        get => string.IsNullOrEmpty(MetricsJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetricsJson);
        set => MetricsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? NotificationRecipientsList
    {
        get => string.IsNullOrEmpty(NotificationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationRecipients);
        set => NotificationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? QualityIssuesList
    {
        get => string.IsNullOrEmpty(QualityIssues) ? null : JsonConvert.DeserializeObject<List<object>>(QualityIssues);
        set => QualityIssues = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? BusinessMetricsObject
    {
        get => string.IsNullOrEmpty(BusinessMetrics) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(BusinessMetrics);
        set => BusinessMetrics = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AuditTrailList
    {
        get => string.IsNullOrEmpty(AuditTrail) ? null : JsonConvert.DeserializeObject<List<object>>(AuditTrail);
        set => AuditTrail = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? SystemConfigurationObject
    {
        get => string.IsNullOrEmpty(SystemConfiguration) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(SystemConfiguration);
        set => SystemConfiguration = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? GeneratedReportIdsList
    {
        get => string.IsNullOrEmpty(GeneratedReportIds) ? null : JsonConvert.DeserializeObject<List<string>>(GeneratedReportIds);
        set => GeneratedReportIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ProcessedFileIdsList
    {
        get => string.IsNullOrEmpty(ProcessedFileIds) ? null : JsonConvert.DeserializeObject<List<string>>(ProcessedFileIds);
        set => ProcessedFileIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? CreatedExportIdsList
    {
        get => string.IsNullOrEmpty(CreatedExportIds) ? null : JsonConvert.DeserializeObject<List<string>>(CreatedExportIds);
        set => CreatedExportIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AlertsTriggeredList
    {
        get => string.IsNullOrEmpty(AlertsTriggered) ? null : JsonConvert.DeserializeObject<List<object>>(AlertsTriggered);
        set => AlertsTriggered = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsSuccessful => Status == ScheduleExecutionStatus.Completed;
    public bool IsFailed => Status == ScheduleExecutionStatus.Failed;
    public bool IsRunning => Status == ScheduleExecutionStatus.Running;
    public bool IsCompleted => Status == ScheduleExecutionStatus.Completed || Status == ScheduleExecutionStatus.Failed || Status == ScheduleExecutionStatus.Cancelled;
    
    public decimal GetCompletionPercentage()
    {
        if (TotalItems.HasValue && TotalItems.Value > 0)
        {
            return (decimal)(ItemsProcessed ?? 0) / TotalItems.Value * 100;
        }
        return ProgressPercentage ?? 0;
    }
    
    public TimeSpan GetEstimatedTimeRemaining()
    {
        if (!StartTime.HasValue || !TotalItems.HasValue || TotalItems.Value == 0 || ItemsProcessed == 0)
            return TimeSpan.Zero;
            
        var elapsed = DateTime.UtcNow - StartTime.Value;
        var progressRatio = (decimal)ItemsProcessed.Value / TotalItems.Value;
        
        if (progressRatio == 0) return TimeSpan.Zero;
        
        var estimatedTotal = TimeSpan.FromTicks((long)(elapsed.Ticks / progressRatio));
        return estimatedTotal - elapsed;
    }
    
    // Navigation Properties
    public virtual Schedule Schedule { get; set; } = null!;
}