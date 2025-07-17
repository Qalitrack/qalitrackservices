using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OperationalDataService.Core.Entities;

public class Process : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ProcessName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OperationalId { get; set; } = string.Empty;
    
    [Required]
    public ProcessType ProcessType { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public ProcessStatus Status { get; set; } = ProcessStatus.NotStarted;
    
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 1;
    
    [StringLength(50)]
    public string? PreviousProcessId { get; set; }
    
    [StringLength(50)]
    public string? NextProcessId { get; set; }
    
    [Range(0, 100)]
    public decimal ProgressPercentage { get; set; } = 0;
    
    [StringLength(50)]
    public string? AssignedUserId { get; set; }
    
    [StringLength(100)]
    public string? AssignedUserName { get; set; }
    
    public TimeSpan? EstimatedDuration { get; set; }
    public TimeSpan? ActualDuration { get; set; }
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    // Automation properties
    public bool IsAutomated { get; set; } = false;
    public bool RequiresApproval { get; set; } = false;
    public bool IsParallel { get; set; } = false;
    
    [StringLength(50)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    // JSON properties for flexible workflow data
    public string? WorkflowDataJson { get; set; }
    public string? ValidationRulesJson { get; set; }
    public string? ConfigurationJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? WorkflowData
    {
        get => string.IsNullOrEmpty(WorkflowDataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(WorkflowDataJson);
        set => WorkflowDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ValidationRules
    {
        get => string.IsNullOrEmpty(ValidationRulesJson) ? null : JsonConvert.DeserializeObject<List<string>>(ValidationRulesJson);
        set => ValidationRulesJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Operational Operation { get; set; } = null!;
    public virtual Process? PreviousProcess { get; set; }
    public virtual Process? NextProcess { get; set; }
    public virtual ICollection<Monitoring> MonitoringRecords { get; set; } = new List<Monitoring>();
}

public enum ProcessType
{
    DataInput = 1,
    DataValidation = 2,
    DataProcessing = 3,
    Calculation = 4,
    Integration = 5,
    Notification = 6,
    Approval = 7,
    Audit = 8,
    Backup = 9,
    Cleanup = 10,
    Reporting = 11,
    Synchronization = 12
}

public enum ProcessStatus
{
    NotStarted = 1,
    InProgress = 2,
    Paused = 3,
    WaitingApproval = 4,
    Completed = 5,
    Failed = 6,
    Cancelled = 7,
    Skipped = 8
}