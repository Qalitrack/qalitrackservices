using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetWithPermissionsAsync(string roleId);
    Task<IEnumerable<Role>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<Role>> GetSystemRolesAsync();
    Task<IEnumerable<Role>> GetDefaultRolesAsync();
    Task<IEnumerable<Role>> GetActiveRolesAsync();
    Task<bool> IsRoleNameAvailableAsync(string name, string? organizationId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(string roleId);
    Task AddRolePermissionAsync(string roleId, string permissionId);
    Task RemoveRolePermissionAsync(string roleId, string permissionId);
    Task UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissionIds);
}