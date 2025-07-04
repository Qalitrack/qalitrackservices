using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class ChangeRecord : BaseEntity
{
    public string ChangeId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public ChangeOperation Operation { get; set; }
    public string? OldDataJson { get; set; }
    public string? NewDataJson { get; set; }
    public string? DeltaJson { get; set; }
    public DateTime ChangeTimestamp { get; set; }
    public string SourceSiteId { get; set; } = string.Empty;
    public string? TargetSiteId { get; set; }
    public SyncStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryTime { get; set; }
    public string? MetadataJson { get; set; }
    public string? ChecksumHash { get; set; }
    public long? SequenceNumber { get; set; }
    
    // Navigation properties
    public virtual SyncSession SyncSession { get; set; } = null!;
    public virtual ICollection<SyncConflict> RelatedConflicts { get; set; } = new List<SyncConflict>();
}