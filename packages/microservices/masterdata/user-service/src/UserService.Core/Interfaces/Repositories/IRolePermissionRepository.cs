using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<string?> GetByRoleAndPermissionAsync(string roleId, string permissionId);
        Task AddAsync(RolePermission rolePermission);
        Task SaveChangesAsync();
    }
}