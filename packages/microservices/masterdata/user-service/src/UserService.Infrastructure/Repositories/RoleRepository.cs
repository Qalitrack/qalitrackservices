using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role?> GetWithPermissionsAsync(string roleId)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<IEnumerable<Role>> GetByOrganizationAsync(string organizationId)
    {
        return await _dbSet
            .Where(r => r.OrganizationId == organizationId || r.Type == RoleType.System)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetSystemRolesAsync()
    {
        return await _dbSet
            .Where(r => r.Type == RoleType.System && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetDefaultRolesAsync()
    {
        return await _dbSet
            .Where(r => r.IsDefault && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<bool> IsRoleNameAvailableAsync(string name, string? organizationId)
    {
        var query = _dbSet.Where(r => r.Name == name);
        
        if (organizationId != null)
        {
            query = query.Where(r => r.OrganizationId == organizationId);
        }
        else
        {
            query = query.Where(r => r.Type == RoleType.System);
        }

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(string roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.IsActive)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task AddRolePermissionAsync(string roleId, string permissionId)
    {
        var existingRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (existingRolePermission == null)
        {
            var rolePermission = new RolePermission
            {
                Id = Guid.NewGuid().ToString(),
                RoleId = roleId,
                PermissionId = permissionId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.RolePermissions.AddAsync(rolePermission);
        }
        else if (!existingRolePermission.IsActive)
        {
            existingRolePermission.IsActive = true;
            existingRolePermission.UpdatedAt = DateTime.UtcNow;
            _context.RolePermissions.Update(existingRolePermission);
        }
    }

    public async Task RemoveRolePermissionAsync(string roleId, string permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            rolePermission.IsActive = false;
            rolePermission.UpdatedAt = DateTime.UtcNow;
            _context.RolePermissions.Update(rolePermission);
        }
    }

    public async Task UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissionIds)
    {
        // Get current role permissions
        var currentRolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        // Deactivate all current permissions
        foreach (var rp in currentRolePermissions)
        {
            rp.IsActive = false;
            rp.UpdatedAt = DateTime.UtcNow;
        }

        // Add or reactivate specified permissions
        foreach (var permissionId in permissionIds)
        {
            var existingRolePermission = currentRolePermissions
                .FirstOrDefault(rp => rp.PermissionId == permissionId);

            if (existingRolePermission != null)
            {
                existingRolePermission.IsActive = true;
                existingRolePermission.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newRolePermission = new RolePermission
                {
                    Id = Guid.NewGuid().ToString(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.RolePermissions.AddAsync(newRolePermission);
            }
        }

        _context.RolePermissions.UpdateRange(currentRolePermissions);
    }
}