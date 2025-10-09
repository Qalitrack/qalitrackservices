using UserService.Core.Entities;
using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<bool> DeleteByRoleAndPermissionAsync(string roleId, string permissionId);
        
        Task<bool> AssignOrRestorePermissionToRoleAsync(string roleId, string permissionId);
        Task AddAsync(RolePermission rolePermission);
        Task SaveChangesAsync();
        Task<PagedResult<RolePermission>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}