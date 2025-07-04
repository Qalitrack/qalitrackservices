using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<UserSession?> GetBySessionIdAsync(string sessionId);
    Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<UserSession>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId);
    Task<UserSession?> GetActiveSessionByRefreshTokenAsync(string refreshToken);
    Task DeactivateSessionAsync(string sessionId);
    Task DeactivateAllUserSessionsAsync(string userId);
    Task DeactivateExpiredSessionsAsync();
    Task<int> GetActiveSessionCountAsync(string userId);
    Task UpdateLastAccessedAsync(string sessionId);
    Task CleanupExpiredSessionsAsync();
}