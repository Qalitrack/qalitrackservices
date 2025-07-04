using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class SyncConflict : BaseEntity
{
    public string ConflictId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public ConflictStatus Status { get; set; }
    public string ConflictType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceDataJson { get; set; } = string.Empty;
    public string TargetDataJson { get; set; } = string.Empty;
    public string? ResolvedDataJson { get; set; }
    public ConflictResolutionStrategy ResolutionStrategy { get; set; }
    public string? ResolutionReason { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string SourceSiteId { get; set; } = string.Empty;
    public string TargetSiteId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime ConflictDetectedAt { get; set; }
    
    // Navigation properties
    public virtual SyncSession SyncSession { get; set; } = null!;
    public virtual ICollection<ChangeRecord> RelatedChanges { get; set; } = new List<ChangeRecord>();
}