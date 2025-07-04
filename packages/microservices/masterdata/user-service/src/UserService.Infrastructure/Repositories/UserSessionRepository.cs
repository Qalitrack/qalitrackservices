using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<UserSession?> GetBySessionIdAsync(string sessionId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
    }

    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId)
    {
        return await _dbSet
            .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastAccessedAt ?? s.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserSession?> GetActiveSessionByRefreshTokenAsync(string refreshToken)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && 
                                s.IsActive && 
                                s.ExpiresAt > DateTime.UtcNow);
    }

    public async Task DeactivateSessionAsync(string sessionId)
    {
        var session = await _dbSet.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session != null)
        {
            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(session);
        }
    }

    public async Task DeactivateAllUserSessionsAsync(string userId)
    {
        var sessions = await _dbSet
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
        }

        _dbSet.UpdateRange(sessions);
    }

    public async Task DeactivateExpiredSessionsAsync()
    {
        var expiredSessions = await _dbSet
            .Where(s => s.IsActive && s.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
        }

        _dbSet.UpdateRange(expiredSessions);
    }

    public async Task<int> GetActiveSessionCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow);
    }

    public async Task UpdateLastAccessedAsync(string sessionId)
    {
        var session = await _dbSet.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session != null)
        {
            session.LastAccessedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(session);
        }
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        var expiredSessions = await _dbSet
            .Where(s => s.ExpiresAt <= DateTime.UtcNow.AddDays(-7)) // Keep expired sessions for 7 days
            .ToListAsync();

        _dbSet.RemoveRange(expiredSessions);
    }
}