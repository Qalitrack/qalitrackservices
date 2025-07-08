using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IPermissionService
{
    Task<PermissionDto?> GetPermissionByIdAsync(string permissionId);
    Task<PermissionDto?> GetPermissionByNameAsync(string permissionName);
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync();
    Task<IEnumerable<PermissionDto>> GetPermissionsByResourceAsync(string resource);
    Task<IEnumerable<PermissionDto>> GetPermissionsByActionAsync(string action);
    Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(string userId);
    Task<IEnumerable<PermissionDto>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId);
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto request);
    Task<PermissionDto> UpdatePermissionAsync(string permissionId, UpdatePermissionDto request);
    Task<bool> DeletePermissionAsync(string permissionId);
    Task<bool> IsPermissionNameAvailableAsync(string permissionName);
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);
    Task<bool> UserHasPermissionAsync(string userId, string resource, string action);
    Task<bool> UserHasPermissionInOrganizationAsync(string userId, string permissionName, string organizationId);
}