using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Core.Entities
{
    public class Role : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        // Seeded system roles (e.g. Admin) are referenced by name in [Authorize] attributes
        // across the backend — renaming/deleting them would silently break authorization.
        [Required]
        public bool IsSystem { get; set; } = false;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        // Computed properties
        [NotMapped]
        public int UserCount => UserRoles?.Count ?? 0;
    }
}