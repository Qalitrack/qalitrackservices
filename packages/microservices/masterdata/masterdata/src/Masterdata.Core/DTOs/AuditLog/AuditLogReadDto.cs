using System;
using System.Text.Json;

namespace Masterdata.Core.DTOs.AuditLog;

public class AuditLogReadDto
{
    public string Id { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public JsonDocument? OldValues { get; set; }
    public JsonDocument? NewValues { get; set; }
    public string[]? AffectedProperties { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
