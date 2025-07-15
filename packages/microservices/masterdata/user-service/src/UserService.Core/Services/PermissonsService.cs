using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class PermissionsService : IPermissionsService
{
    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<PermissionDto?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<PermissionDto> CreateAsync(CreatePermissionDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<PermissionDto?> UpdateAsync(string id, UpdatePermissionDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DoesPermissionExistAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<RoleDto>> GetRolesForPermissionAsync(string permissionId)
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