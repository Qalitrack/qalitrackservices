using UserService.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserService.Core.Interfaces
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        // Get all permissions for a specific role
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId);

        // Assign a permission to a role
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);

        // Remove a permission from a role
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);
        Task<string?> GetByRoleAndPermissionAsync(string roleId, string permissionId);
        Task AddAsync(RolePermission rolePermission);
        Task SaveChangesAsync();
    }
}