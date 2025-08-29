using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.UserRole;

namespace UserService.Core.Interfaces.Services
{
    public interface IUserRoleService
    {
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    
        Task<ServiceResult> AddUserToRoleAsync(string userId, string roleId);
        Task<ServiceResult> RemoveUserFromRoleAsync(string userId, string roleId);

        Task<PagedResult<UserRoleDto>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}