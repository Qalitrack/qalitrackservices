using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        // Get a role by its name
        Task<Role?> GetByNameAsync(string roleName);

        // Check if a role with the given name exists
        Task<bool> DoesRoleExistAsync(string roleName);

        // Get all permissions associated with a specific role
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId);
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);
        Task<object> AddAsync(Role role);
        Task SaveChangesAsync();
        Task GetByIdWithPermissionsAsync(string roleId);
        Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<string> select);
    }
}