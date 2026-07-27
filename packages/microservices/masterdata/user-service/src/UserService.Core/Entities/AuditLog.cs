namespace UserService.Core.Entities;

// One row per mutating HTTP request (GET is never logged), captured by the
// gateway and forwarded here since the gateway itself has no database.
public class AuditLog : BaseEntity
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public long DurationMs { get; set; }
}
