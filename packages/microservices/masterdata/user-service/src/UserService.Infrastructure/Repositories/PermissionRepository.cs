using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource)
    {
        return await _dbSet
            .Where(p => p.Resource == resource && p.IsActive)
            .OrderBy(p => p.Action)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByActionAsync(string action)
    {
        return await _dbSet
            .Where(p => p.Action == action && p.IsActive)
            .OrderBy(p => p.Resource)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByScopeAsync(PermissionScope scope)
    {
        return await _dbSet
            .Where(p => p.Scope == scope && p.IsActive)
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetActivePermissionsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync();
    }

    public async Task<bool> IsPermissionNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.IsActive && rp.Permission.IsActive)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive && 
                        (ur.OrganizationId == organizationId || ur.Role.Type == RoleType.System))
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.IsActive && rp.Permission.IsActive)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }
}