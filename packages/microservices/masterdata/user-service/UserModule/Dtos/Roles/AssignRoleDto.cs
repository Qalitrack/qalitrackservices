using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Roles;

public class AssignRoleDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
}