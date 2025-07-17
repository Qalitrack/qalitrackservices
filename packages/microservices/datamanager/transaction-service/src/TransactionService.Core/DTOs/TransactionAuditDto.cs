namespace TransactionService.Core.DTOs;

public class TransactionAuditDto
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public DateTime AuditDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string? Reason { get; set; }
    public string AuditAction { get; set; } = string.Empty;
}

public class CreateAuditRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public string? Reason { get; set; }
    public string AuditAction { get; set; } = string.Empty;
}