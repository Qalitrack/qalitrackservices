using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IRoleService
{
    Task<RoleDto?> GetRoleByIdAsync(string roleId);
    Task<RoleDto?> GetRoleByNameAsync(string roleName);
    Task<RoleDto?> GetRoleWithPermissionsAsync(string roleId);
    Task<IEnumerable<RoleDto>> GetRolesAsync();
    Task<IEnumerable<RoleDto>> GetRolesByOrganizationAsync(string organizationId);
    Task<IEnumerable<RoleDto>> GetSystemRolesAsync();
    Task<IEnumerable<RoleDto>> GetDefaultRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleDto request);
    Task<RoleDto> UpdateRoleAsync(string roleId, UpdateRoleDto request);
    Task<bool> DeleteRoleAsync(string roleId);
    Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);
    Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);
    Task<bool> UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissionIds);
    Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(string roleId);
    Task<bool> IsRoleNameAvailableAsync(string roleName, string? organizationId);
}