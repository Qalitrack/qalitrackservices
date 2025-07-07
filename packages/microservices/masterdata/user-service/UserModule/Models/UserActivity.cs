using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

[Table("UserActivities")]
public class UserActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string ActivityType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
    [StringLength(100)]
    public string? IpAddress { get; set; }
    [StringLength(200)]
    public string? UserAgent { get; set; }
    [StringLength(200)]
    public Guid? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}