using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class PermissionsRepository(UserServiceDbContext dbContext)
        : Repository<Permission>(dbContext), IPermissionsRepository
    {
        private readonly UserServiceDbContext _dbContext = dbContext;

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _dbContext.Permissions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(string id, bool includeRelated)
        {
            var query = _dbContext.Permissions.AsQueryable();
            
            if (includeRelated)
            {
                query = query
                    .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role);
            }
            
            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var permission = await _dbContext.Permissions.FindAsync(id);
            if (permission == null)
                return false;

            _dbContext.Permissions.Remove(permission);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // public async Task<Permission?> GetByNameAsync(string name)
        // {
        //     return await _dbContext.Permissions
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
        // }

        public async Task<bool> DoesPermissionExistAsync(string name)
        {
            return await _dbContext.Permissions
                .AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        // public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
        // {
        //     return await _dbContext.RolePermissions
        //         .Where(rp => rp.RoleId == roleId)
        //         .Select(rp => rp.Permission)
        //         .ToListAsync();
        // }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
        {
            // Check if the assignment already exists
            var exists = await _dbContext.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (exists)
                return true; // Already assigned

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            await _dbContext.RolePermissions.AddAsync(rolePermission);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
        {
            var rolePermission = await _dbContext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
                return false;

            _dbContext.RolePermissions.Remove(rolePermission);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Role>> GetRolesForPermissionAsync(string permissionId)
        {
            return await _dbContext.RolePermissions
                .Include(rp => rp.Role)
                .Where(rp => rp.PermissionId == permissionId)
                .Select(rp => rp.Role)
                .ToListAsync();
        }
        
    }
}