using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Roles
{
    public class UpdateRoleDto
    {
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}