using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class RoleRepository(UserServiceDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRoleRepository
    {
        private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Where(r => !r.IsDeleted)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(string id, bool b)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Role> CreateAsync(Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            //use  the create method in the base class
            return await CreateAsync(entity);
        }

        public async Task<Role?> UpdateAsync(Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existingRole = await _context.Roles
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == entity.Id && !r.IsDeleted);

            if (existingRole == null)
                return null;

            // Only update specific fields to avoid constraint issues
            existingRole.Name = entity.Name;
            existingRole.Description = entity.Description; 
            existingRole.IsActive = entity.IsActive;
            existingRole.UpdatedAt = DateTime.UtcNow;
            existingRole.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User) ?? "System";
            
            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (role == null)
                return false;

            if (role.UserRoles != null && role.UserRoles.Any())
            {
                throw new InvalidOperationException("Cannot delete role that is assigned to users.");
            }
        
            role.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.Trim().ToLower() && !r.IsDeleted);
        }

        public async Task<bool> DoesRoleExistAsync(string roleName)
        {
            return await _context.Roles
                .AnyAsync(r => r.Name == roleName && !r.IsDeleted);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }
        

        public async Task<object> AddAsync(Role role)
        {
            //use the create method in the base class
            role.CreatedAt = DateTime.UtcNow;
            role.CreatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
            role.UpdatedAt = role.CreatedAt;
            role.UpdatedBy = role.CreatedBy;
            return await CreateAsync(role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<string> select)
        {
            return await _context.Roles
                .Where(r => select.Contains(r.Id))
                .ToListAsync(); 
        }

        public async Task<Role?> GetRoleWithPermissionsAsync(string roleName)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }
    }
}