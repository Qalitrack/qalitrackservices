using System.Text.Json;

namespace Masterdata.Core.DTOs.AuditLog;

public class CreateAuditLogDto
{
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!; // "Create", "Update", "Delete"
    public JsonDocument? OldValues { get; set; }
    public JsonDocument? NewValues { get; set; }
    public string[]? AffectedProperties { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
}
