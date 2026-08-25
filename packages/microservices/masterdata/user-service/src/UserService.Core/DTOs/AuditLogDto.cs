namespace UserService.Core.DTOs;

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
}

// Posted by the gateway — no auth on this endpoint (see AuditLogsController),
// so keep it a narrow, purpose-built shape rather than accepting an entity directly.
public class CreateAuditLogDto
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public long DurationMs { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
}

public class AuditLogVerifyResultDto
{
    public bool Valid { get; set; }
    public long CheckedCount { get; set; }
    public long? FirstBrokenSequenceNumber { get; set; }
}
