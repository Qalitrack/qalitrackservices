using UserService.Core.DTOs.Roles;

namespace UserService.Core.DTOs.Permissions;

public class PermissionWithRolesDto : PermissionDto
{
    public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
}
