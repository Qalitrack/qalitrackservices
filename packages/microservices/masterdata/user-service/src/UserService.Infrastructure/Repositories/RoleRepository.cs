using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class RoleRepository : IRepository<Role>, IRoleRepository
    {
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Role?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Role> CreateAsync(Role entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Role?> UpdateAsync(Role entity)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DoesRoleExistAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
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
}
