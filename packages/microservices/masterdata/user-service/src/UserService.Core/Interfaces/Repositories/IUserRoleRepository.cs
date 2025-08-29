using UserService.Core.Entities;
using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IUserRoleRepository
    {
     
        Task<IEnumerable<UserRole>> GetAllAsync(CancellationToken cancellationToken = default); 
        Task<bool> AssignRoleToUserAsync(
           string userId, 
           string roleId, CancellationToken cancellationToken = default);
        Task<bool> RemoveRoleFromUserAsync(
            string userId, 
            string roleId, 
            CancellationToken cancellationToken = default);

        Task<int> RemoveRoleFromAllUsersAsync(string id);

        Task<PagedResult<UserRole>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}