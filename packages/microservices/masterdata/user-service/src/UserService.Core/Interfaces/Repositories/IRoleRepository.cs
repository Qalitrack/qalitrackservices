using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        // Get a role by its name
        Task<Role?> GetByNameAsync(string roleName);


        // Get all permissions associated with a specific role
        Task<object> AddAsync(Role role);
        Task SaveChangesAsync();
        
        Task<Role?> GetRoleWithPermissionsAsync(string roleName);
    }
}