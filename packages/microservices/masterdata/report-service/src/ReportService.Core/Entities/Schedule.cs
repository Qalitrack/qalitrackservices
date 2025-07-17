using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ReportService.Core.Entities;

public class Schedule : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ScheduleName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public ScheduleType ScheduleType { get; set; }
    
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Active;
    
    // Basic Scheduling
    public bool IsActive { get; set; } = true;
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    [StringLength(20)]
    public string? TimeZone { get; set; } = "UTC";
    
    // Recurrence Configuration
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Once;
    
    public int RecurrenceInterval { get; set; } = 1; // Every N periods
    
    [StringLength(100)]
    public string? CronExpression { get; set; } // For advanced scheduling
    
    // Weekly Configuration
    public bool Monday { get; set; } = false;
    public bool Tuesday { get; set; } = false;
    public bool Wednesday { get; set; } = false;
    public bool Thursday { get; set; } = false;
    public bool Friday { get; set; } = false;
    public bool Saturday { get; set; } = false;
    public bool Sunday { get; set; } = false;
    
    // Monthly Configuration
    public int? DayOfMonth { get; set; } // 1-31
    public int? WeekOfMonth { get; set; } // 1-5 (5 = last week)
    public DayOfWeek? DayOfWeek { get; set; }
    
    // Quarterly Configuration
    public int? MonthOfQuarter { get; set; } // 1-3
    
    // Yearly Configuration
    public int? MonthOfYear { get; set; } // 1-12
    
    // Execution Configuration
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan? RetryInterval { get; set; } = TimeSpan.FromMinutes(15);
    
    public TimeSpan? TimeoutDuration { get; set; } = TimeSpan.FromHours(2);
    
    public bool ContinueOnError { get; set; } = false;
    
    [StringLength(1000)]
    public string? FailureNotificationEmails { get; set; } // JSON array
    
    // Next Execution
    public DateTime? NextRunDate { get; set; }
    public DateTime? LastRunDate { get; set; }
    public DateTime? LastSuccessfulRunDate { get; set; }
    
    public ScheduleExecutionStatus LastExecutionStatus { get; set; } = ScheduleExecutionStatus.NotStarted;
    
    [StringLength(1000)]
    public string? LastExecutionError { get; set; }
    
    public TimeSpan? LastExecutionDuration { get; set; }
    
    // Run History Summary
    public int TotalRuns { get; set; } = 0;
    public int SuccessfulRuns { get; set; } = 0;
    public int FailedRuns { get; set; } = 0;
    public int SkippedRuns { get; set; } = 0;
    
    public TimeSpan? AverageExecutionDuration { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    public SchedulePriority Priority { get; set; } = SchedulePriority.Medium;
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(1000)]
    public string? BusinessJustification { get; set; }
    
    // Dependencies
    [StringLength(1000)]
    public string? DependentScheduleIds { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? PrerequisiteScheduleIds { get; set; } // JSON array
    
    public bool WaitForDependencies { get; set; } = false;
    public TimeSpan? DependencyTimeout { get; set; } = TimeSpan.FromHours(1);
    
    // Notification Configuration
    public bool NotifyOnSuccess { get; set; } = false;
    public bool NotifyOnFailure { get; set; } = true;
    public bool NotifyOnStart { get; set; } = false;
    
    [StringLength(1000)]
    public string? NotificationEmails { get; set; } // JSON array
    
    [StringLength(500)]
    public string? NotificationWebhookUrl { get; set; }
    
    [StringLength(1000)]
    public string? NotificationMessage { get; set; }
    
    // Resource Management
    public int? MaxConcurrentExecutions { get; set; } = 1;
    
    [StringLength(100)]
    public string? ExecutionQueue { get; set; }
    
    public int? MemoryLimitMB { get; set; }
    public int? CpuLimitPercentage { get; set; }
    
    // Execution Environment
    [StringLength(1000)]
    public string? EnvironmentVariables { get; set; } // JSON object
    
    [StringLength(1000)]
    public string? ExecutionParameters { get; set; } // JSON object
    
    [StringLength(500)]
    public string? WorkingDirectory { get; set; }
    
    // Monitoring and Alerting
    public bool EnableMonitoring { get; set; } = true;
    
    public TimeSpan? ExpectedDuration { get; set; }
    public decimal? ExpectedDurationTolerancePercentage { get; set; } = 20;
    
    public bool AlertOnLongRunning { get; set; } = true;
    public bool AlertOnUnexpectedFailure { get; set; } = true;
    
    // Audit and Compliance
    public bool RequiresApproval { get; set; } = false;
    
    [StringLength(50)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovalDate { get; set; }
    
    [StringLength(1000)]
    public string? ApprovalComments { get; set; }
    
    public bool IsComplianceRequired { get; set; } = false;
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? FailureNotificationEmailsList
    {
        get => string.IsNullOrEmpty(FailureNotificationEmails) ? null : JsonConvert.DeserializeObject<List<string>>(FailureNotificationEmails);
        set => FailureNotificationEmails = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DependentScheduleIdsList
    {
        get => string.IsNullOrEmpty(DependentScheduleIds) ? null : JsonConvert.DeserializeObject<List<string>>(DependentScheduleIds);
        set => DependentScheduleIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? PrerequisiteScheduleIdsList
    {
        get => string.IsNullOrEmpty(PrerequisiteScheduleIds) ? null : JsonConvert.DeserializeObject<List<string>>(PrerequisiteScheduleIds);
        set => PrerequisiteScheduleIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? NotificationEmailsList
    {
        get => string.IsNullOrEmpty(NotificationEmails) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationEmails);
        set => NotificationEmails = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? EnvironmentVariablesObject
    {
        get => string.IsNullOrEmpty(EnvironmentVariables) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(EnvironmentVariables);
        set => EnvironmentVariables = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ExecutionParametersObject
    {
        get => string.IsNullOrEmpty(ExecutionParameters) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ExecutionParameters);
        set => ExecutionParameters = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods for day of week configuration
    public List<DayOfWeek> GetScheduledDaysOfWeek()
    {
        var days = new List<DayOfWeek>();
        if (Sunday) days.Add(System.DayOfWeek.Sunday);
        if (Monday) days.Add(System.DayOfWeek.Monday);
        if (Tuesday) days.Add(System.DayOfWeek.Tuesday);
        if (Wednesday) days.Add(System.DayOfWeek.Wednesday);
        if (Thursday) days.Add(System.DayOfWeek.Thursday);
        if (Friday) days.Add(System.DayOfWeek.Friday);
        if (Saturday) days.Add(System.DayOfWeek.Saturday);
        return days;
    }
    
    public void SetScheduledDaysOfWeek(List<DayOfWeek> days)
    {
        Sunday = days.Contains(System.DayOfWeek.Sunday);
        Monday = days.Contains(System.DayOfWeek.Monday);
        Tuesday = days.Contains(System.DayOfWeek.Tuesday);
        Wednesday = days.Contains(System.DayOfWeek.Wednesday);
        Thursday = days.Contains(System.DayOfWeek.Thursday);
        Friday = days.Contains(System.DayOfWeek.Friday);
        Saturday = days.Contains(System.DayOfWeek.Saturday);
    }
    
    // Success rate calculation
    public decimal GetSuccessRate()
    {
        if (TotalRuns == 0) return 0;
        return (decimal)SuccessfulRuns / TotalRuns * 100;
    }
    
    // Navigation Properties
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    public virtual ICollection<ScheduleExecution> Executions { get; set; } = new List<ScheduleExecution>();
}

public enum ScheduleType
{
    ReportGeneration = 1,
    DataRefresh = 2,
    Maintenance = 3,
    Backup = 4,
    DataSync = 5,
    Notification = 6,
    Cleanup = 7,
    Custom = 8
}

public enum ScheduleStatus
{
    Draft = 1,
    Active = 2,
    Paused = 3,
    Suspended = 4,
    Completed = 5,
    Expired = 6,
    Error = 7
}

public enum RecurrenceType
{
    Once = 1,
    Minutely = 2,
    Hourly = 3,
    Daily = 4,
    Weekly = 5,
    Monthly = 6,
    Quarterly = 7,
    Yearly = 8,
    Custom = 9
}

public enum SchedulePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ScheduleExecutionStatus
{
    NotStarted = 1,
    Queued = 2,
    Running = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6,
    Timeout = 7,
    Skipped = 8
}