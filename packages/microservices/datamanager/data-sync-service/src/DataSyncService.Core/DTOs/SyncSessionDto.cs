using DataSyncService.Core.Enums;

namespace DataSyncService.Core.DTOs;

public class SyncSessionDto
{
    public int Id { get; set; }
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
    public TimeSpan? Duration { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSyncSessionRequest
{
    public string SourceSiteId { get; set; } = string.Empty;
    public string TargetSiteId { get; set; } = string.Empty;
    public SyncDirection Direction { get; set; }
    public SyncMode Mode { get; set; }
    public SyncPriority Priority { get; set; }
    public string? MetadataJson { get; set; }
}

public class UpdateSyncSessionRequest
{
    public SyncStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public int ConflictCount { get; set; }
}