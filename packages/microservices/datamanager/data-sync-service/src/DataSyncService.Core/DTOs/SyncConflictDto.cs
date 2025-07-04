using DataSyncService.Core.Enums;

namespace DataSyncService.Core.DTOs;

public class SyncConflictDto
{
    public int Id { get; set; }
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
    public DateTime ConflictDetectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ResolveConflictRequest
{
    public ConflictResolutionStrategy ResolutionStrategy { get; set; }
    public string? ResolvedDataJson { get; set; }
    public string? ResolutionReason { get; set; }
    public string ResolvedBy { get; set; } = string.Empty;
}

public class ConflictResolutionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SyncConflictDto? ResolvedConflict { get; set; }
}