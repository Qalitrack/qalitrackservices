using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

public class UserRole
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

}