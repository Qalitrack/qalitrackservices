namespace UserService.Core.Entities;

// One row per mutating HTTP request, plus reads of sensitive entities,
// captured by the gateway and forwarded here since the gateway itself has
// no database.
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

    // Structured classification derived from the path (see gateway's ClassifyPath).
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;

    // Tamper-evidence hash chain. Id (BaseEntity) is a client-generated GUID and
    // isn't ordered, so SequenceNumber (DB identity) is the real chain order.
    public long SequenceNumber { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
