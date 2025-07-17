using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OperationalDataService.Core.Entities;

public class Operational : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string OperationName { get; set; } = string.Empty;
    
    [Required]
    public OperationType OperationType { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public OperationStatus Status { get; set; } = OperationStatus.Active;
    
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    
    [StringLength(50)]
    public string? ResponsibleUserId { get; set; }
    
    [StringLength(100)]
    public string? ResponsibleUserName { get; set; }
    
    public int Priority { get; set; } = 1; // 1 = Low, 2 = Medium, 3 = High, 4 = Critical
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [StringLength(50)]
    public string? ParentOperationId { get; set; }
    
    // JSON properties for flexible data storage
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
    
    // Navigation Properties
    public virtual ICollection<Process> Processes { get; set; } = new List<Process>();
    public virtual ICollection<Monitoring> MonitoringRecords { get; set; } = new List<Monitoring>();
    public virtual Operational? ParentOperation { get; set; }
    public virtual ICollection<Operational> SubOperations { get; set; } = new List<Operational>();
}

public enum OperationType
{
    WeighingOperation = 1,
    TransactionProcessing = 2,
    DataCollection = 3,
    Maintenance = 4,
    Calibration = 5,
    Reporting = 6,
    Integration = 7,
    Monitoring = 8,
    Backup = 9,
    SystemTask = 10,
    UserTask = 11
}

public enum OperationStatus
{
    Pending = 1,
    Active = 2,
    InProgress = 3,
    Paused = 4,
    Completed = 5,
    Failed = 6,
    Cancelled = 7,
    Archived = 8
}