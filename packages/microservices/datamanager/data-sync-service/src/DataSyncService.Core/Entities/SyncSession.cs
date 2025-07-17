using System.ComponentModel.DataAnnotations;
using DataSyncService.Core.Enums;
using Newtonsoft.Json;

namespace DataSyncService.Core.Entities;

public class SyncSession : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    
    [StringLength(50)]
    public string? SyncId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string SourceSiteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string TargetSiteId { get; set; } = string.Empty;
    
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    public SyncDirection Direction { get; set; } = SyncDirection.Bidirectional;
    public SyncMode Mode { get; set; } = SyncMode.Incremental;
    public SyncPriority Priority { get; set; } = SyncPriority.Normal;
    
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    public int TotalRecords { get; set; } = 0;
    public int ProcessedRecords { get; set; } = 0;
    public int SuccessfulRecords { get; set; } = 0;
    public int FailedRecords { get; set; } = 0;
    public int ConflictCount { get; set; } = 0;
    
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    public decimal ProgressPercentage => TotalRecords > 0 ? (decimal)ProcessedRecords / TotalRecords * 100 : 0;
    
    // JSON properties for flexible data storage
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation properties
    public virtual Sync? Sync { get; set; }
    public virtual SyncSite SourceSite { get; set; } = null!;
    public virtual SyncSite TargetSite { get; set; } = null!;
    public virtual ICollection<SyncLog> SyncLogs { get; set; } = new List<SyncLog>();
    public virtual ICollection<ChangeRecord> ChangeRecords { get; set; } = new List<ChangeRecord>();
    public virtual ICollection<SyncConflict> SyncConflicts { get; set; } = new List<SyncConflict>();
    public virtual ICollection<Status> StatusHistory { get; set; } = new List<Status>();
}