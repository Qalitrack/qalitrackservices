using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class PermissionsRepository(
        UserServiceDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,ILogger<PermissionsRepository> logger)
        : Repository<Permission>(dbContext, httpContextAccessor, logger), IPermissionsRepository
    {
        private readonly UserServiceDbContext _dbContext = dbContext;
        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _dbContext.Permissions
                .Where(p => !p.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(string id, bool includeRelated)
        {
            var query = _dbContext.Permissions
                .Where(p => !p.IsDeleted && p.Id == id);
            
            if (includeRelated)
            {
                query = query
                    .Include(p => p.RolePermissions.Where(rp => !rp.IsDeleted))
                    .ThenInclude(rp => rp.Role);
            }
            
            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public  async Task<bool> DeleteAsync(string id)
        {
            return await base.DeleteAsync(id);
        }
        

        public async Task<bool> DoesPermissionExistAsync(string name)
        {
            return await _dbContext.Permissions
                .Where(p => !p.IsDeleted)
                .AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
        {
            // Check if the assignment already exists
            var exists = await _dbContext.RolePermissions
                .Where(rp => !rp.IsDeleted)
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (exists)
                return true; // Already assigned

            // Create RolePermission using a separate repository or direct creation
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            // If we had access to RolePermissionRepository, we'd use:
            // await rolePermissionRepository.CreateAsync(rolePermission);
            
            // For now, manually set audit fields (consistent with base repository logic)
            var currentUserId = GetCurrentUserId();
            rolePermission.Id = Guid.NewGuid().ToString();
            rolePermission.CreatedAt = DateTime.UtcNow;
            rolePermission.UpdatedAt = DateTime.UtcNow;
            rolePermission.CreatedBy = currentUserId;
            rolePermission.UpdatedBy = currentUserId;
            rolePermission.IsDeleted = false;

            await _dbContext.RolePermissions.AddAsync(rolePermission);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
        {
            var rolePermission = await _dbContext.RolePermissions
                .AsTracking()
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && 
                                          rp.PermissionId == permissionId && 
                                          !rp.IsDeleted);

            if (rolePermission == null)
                return false;

            // Apply the same soft delete logic as base repository
            var currentUserId = GetCurrentUserId();
            rolePermission.IsDeleted = true;
            rolePermission.UpdatedAt = DateTime.UtcNow;
            rolePermission.UpdatedBy = currentUserId;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Role>> GetRolesForPermissionAsync(string permissionId)
        {
            return await _dbContext.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Role.UserRoles.Where(ur => !ur.IsDeleted))
                .ThenInclude(ur => ur.User)
                .Where(rp => rp.PermissionId == permissionId && 
                            !rp.IsDeleted && 
                            !rp.Permission.IsDeleted &&
                            !rp.Role.UserRoles.Any(ur => ur.IsDeleted) &&
                            !rp.Role.IsDeleted)
                .Select(rp => rp.Role)
                .Distinct()
                .ToListAsync();
        }
    }
}