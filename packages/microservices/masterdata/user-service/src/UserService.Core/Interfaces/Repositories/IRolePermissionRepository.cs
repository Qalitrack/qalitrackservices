using UserService.Core.Entities;
using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<string?> GetByRoleAndPermissionAsync(string roleId, string permissionId);
        Task AddAsync(RolePermission rolePermission);
        Task SaveChangesAsync();
        Task<PagedResult<RolePermission>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}