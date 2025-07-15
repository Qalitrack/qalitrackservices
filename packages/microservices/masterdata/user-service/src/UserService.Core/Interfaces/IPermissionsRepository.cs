using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IPermissionsRepository : IRepository<Permission>  // Inheriting from IRepository<Permission>
    {
        // Get a permission by its name
        Task<Permission?> GetByNameAsync(string name);

        // Check if a permission exists by name
        Task<bool> DoesPermissionExistAsync(string name);

        // Get all permissions assigned to a role (via RolePermission)
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId);

        // Assign permission to a role (via RolePermission table)
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);

        // Remove permission from a role
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);
        Task<IEnumerable<Role>> GetRolesForPermissionAsync(string permissionId);
    }
}