using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class SyncSession : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string SourceSiteId { get; set; } = string.Empty;
    public string TargetSiteId { get; set; } = string.Empty;
    public SyncStatus Status { get; set; }
    public SyncDirection Direction { get; set; }
    public SyncMode Mode { get; set; }
    public SyncPriority Priority { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public int ConflictCount { get; set; }
    public string? MetadataJson { get; set; }
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    public decimal ProgressPercentage => TotalRecords > 0 ? (decimal)ProcessedRecords / TotalRecords * 100 : 0;
    
    // Navigation properties
    public virtual ICollection<SyncLog> SyncLogs { get; set; } = new List<SyncLog>();
    public virtual ICollection<ChangeRecord> ChangeRecords { get; set; } = new List<ChangeRecord>();
    public virtual ICollection<SyncConflict> SyncConflicts { get; set; } = new List<SyncConflict>();
}