using System.ComponentModel.DataAnnotations;

namespace UserService.Core.Entities
{
    public class Role : BaseEntity
    {   [Required]
        public string Name { get; set; } = string.Empty;  // The name of the role (e.g., "Admin", "User")
        [Required]
        public string? Description { get; set; }  // An optional description of the role

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();  // Many-to-many with User
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();  // Many-to-many with Permissions
    }
    
}