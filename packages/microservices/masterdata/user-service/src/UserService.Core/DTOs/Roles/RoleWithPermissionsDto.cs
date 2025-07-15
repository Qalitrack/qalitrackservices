using System.Collections.Generic;
using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;

namespace UserService.Core.DTOs;

public class RoleWithPermissionsDto : RoleDto
{
    public IEnumerable<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}
