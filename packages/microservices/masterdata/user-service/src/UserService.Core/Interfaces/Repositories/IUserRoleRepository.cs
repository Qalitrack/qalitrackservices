using UserService.Core.Entities;

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
    }
}