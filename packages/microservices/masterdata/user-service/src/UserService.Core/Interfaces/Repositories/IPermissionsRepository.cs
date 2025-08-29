using UserService.Core.Entities;
using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IPermissionsRepository : IRepository<Permission>  
    {
        Task<bool> DoesPermissionExistAsync(string name);
        
        Task<IEnumerable<Role>> GetRolesForPermissionAsync(string permissionId);
        
        Task<PagedResult<Permission>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}