using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        // Get a role by its name
        Task<Role?> GetByNameAsync(string roleName);

        // Check if a role with the given name exists

        // Get all permissions associated with a specific role
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId);
        Task<object> AddAsync(Role role);
        Task SaveChangesAsync();
        Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<string> select);
        
        Task<Role?> GetRoleWithPermissionsAsync(string roleName);
    }
}