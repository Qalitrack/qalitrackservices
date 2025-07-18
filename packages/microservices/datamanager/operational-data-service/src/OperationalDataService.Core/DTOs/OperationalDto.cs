using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class OperationalDto
{
    public string Id { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public OperationType OperationType { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public OperationStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public int Priority { get; set; }
    public string? Notes { get; set; }
    public string? ParentOperationId { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Related data counts
    public int ProcessCount { get; set; }
    public int MonitoringRecordCount { get; set; }
    public int SubOperationCount { get; set; }
}

public class CreateOperationalRequest
{
    public string OperationName { get; set; } = string.Empty;
    public OperationType OperationType { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 1;
    public string? ResponsibleUserId { get; set; }
    public string? Notes { get; set; }
    public string? ParentOperationId { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateOperationalRequest
{
    public string? OperationName { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public string? ResponsibleUserId { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}