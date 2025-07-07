using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

[Table("AuditLogs")] 
public class AuditLog
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    [Required] [StringLength(100)] public string Action { get; set; } = string.Empty;

    [StringLength(100)] public string? TableName { get; set; }

    public Guid? RecordId { get; set; }

    [Column(TypeName = "jsonb")] public string? OldValues { get; set; }

    [Column(TypeName = "jsonb")] public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")] public virtual User? User { get; set; }
}