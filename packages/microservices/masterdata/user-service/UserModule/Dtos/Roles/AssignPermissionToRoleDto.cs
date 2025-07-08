using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Roles;
public class AssignPermissionToRoleDto
{
    [Required]
    public Guid RoleId { get; set; }
    
    [Required]
    public List<Guid> PermissionIds { get; set; } = new();
}