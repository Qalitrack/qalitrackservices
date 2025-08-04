using UserService.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserService.Core.Interfaces
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<string?> GetByRoleAndPermissionAsync(string roleId, string permissionId);
        Task AddAsync(RolePermission rolePermission);
        Task SaveChangesAsync();
    }
}