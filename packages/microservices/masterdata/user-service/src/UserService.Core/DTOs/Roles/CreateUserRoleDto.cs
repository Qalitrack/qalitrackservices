using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Roles;

public class CreateUserRoleDto
{   [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string RoleId { get; set; } = string.Empty;
}