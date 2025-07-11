using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class RoleService : IRoleService
{
    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<RoleDto?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<RoleDto?> UpdateAsync(string id, UpdateRoleDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DoesRoleExistAsync(string roleName)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(string roleId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
    {
        throw new NotImplementedException();
    }
}