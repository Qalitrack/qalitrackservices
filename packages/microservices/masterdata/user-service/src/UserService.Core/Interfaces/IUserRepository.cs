using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithRolesAsync(string userId);
    Task<User?> GetWithProfileAsync(string userId);
    Task<User?> GetWithOrganizationsAsync(string userId);
    Task<User?> GetCompleteUserAsync(string userId);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<IEnumerable<User>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<User>> GetByRoleAsync(string roleId);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int page, int pageSize);
    Task<int> GetTotalUserCountAsync();
    Task<int> GetActiveUserCountAsync();
    Task UpdateLastLoginAsync(string userId);
    Task UpdatePasswordAsync(string userId, string passwordHash);
    Task UpdateFailedLoginAttemptsAsync(string userId, int attempts);
    Task LockUserAsync(string userId, DateTime lockoutEnd);
    Task UnlockUserAsync(string userId);
}