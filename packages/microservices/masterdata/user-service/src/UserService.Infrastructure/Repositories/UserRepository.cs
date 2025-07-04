using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetWithRolesAsync(string userId)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetWithProfileAsync(string userId)
    {
        return await _dbSet
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetWithOrganizationsAsync(string userId)
    {
        return await _dbSet
            .Include(u => u.OrganizationUsers)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetCompleteUserAsync(string userId)
    {
        return await _dbSet
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.OrganizationUsers)
            .Include(u => u.Sessions.Where(s => s.IsActive))
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return !await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(string organizationId)
    {
        return await _dbSet
            .Include(u => u.OrganizationUsers.Where(ou => ou.OrganizationId == organizationId))
            .Where(u => u.OrganizationUsers.Any(ou => ou.OrganizationId == organizationId))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string roleId)
    {
        return await _dbSet
            .Include(u => u.UserRoles.Where(ur => ur.RoleId == roleId))
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int page, int pageSize)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => 
                u.Username.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm));
        }

        return await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalUserCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        return await _dbSet.CountAsync(u => u.Status == UserStatus.Active);
    }

    public async Task UpdateLastLoginAsync(string userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            _dbSet.Update(user);
        }
    }

    public async Task UpdatePasswordAsync(string userId, string passwordHash)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.PasswordHash = passwordHash;
            user.PasswordChangedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(user);
        }
    }

    public async Task UpdateFailedLoginAttemptsAsync(string userId, int attempts)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.FailedLoginAttempts = attempts;
            user.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(user);
        }
    }

    public async Task LockUserAsync(string userId, DateTime lockoutEnd)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = lockoutEnd;
            user.Status = UserStatus.Locked;
            user.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(user);
        }
    }

    public async Task UnlockUserAsync(string userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = null;
            user.Status = UserStatus.Active;
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(user);
        }
    }
}