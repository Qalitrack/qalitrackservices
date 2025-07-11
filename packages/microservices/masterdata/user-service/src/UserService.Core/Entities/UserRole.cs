using System.ComponentModel.DataAnnotations;

namespace UserService.Core.Entities
{
    public class UserRole : BaseEntity
    {   [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;
        [Required]
        public string RoleId { get; set; } = string.Empty;
        public virtual Role Role { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow; // When the role was assigned
    }
}