using UserModule.Dtos.Users;

namespace UserModule.Services;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<UserPermissionsDto> GetUserPermissionsDetailAsync(Guid userId);
}