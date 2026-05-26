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
using UserService.Core.DTOs.Common;
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

        public async Task<Role?> GetByIdAsync(string id, bool includePermissions = true)
        {
            var query = _context.Roles.AsQueryable();
            
            if (includePermissions)
            {
                query = query
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission);
            }
            
            return await query
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Role> CreateAsync(Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Set audit fields
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User) ?? "System";
            entity.UpdatedAt = entity.CreatedAt;
            entity.UpdatedBy = entity.CreatedBy;
            entity.IsDeleted = false;

            _context.Roles.Add(entity);
            await _context.SaveChangesAsync();
            
            return entity;
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

            // Check if role is assigned to users
            if (role.UserRoles != null && role.UserRoles.Any())
            {
                throw new InvalidOperationException("Cannot delete role that is assigned to users.");
            }
        
            // Soft delete
            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            var role = await _context.Roles
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted);

            if (role == null)
                return false;

            role.IsDeleted = false;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return null;
                
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.Trim().ToLower() && !r.IsDeleted);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return new List<Permission>();
                
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<object> AddAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
                
            return await CreateAsync(role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Role?> GetRoleWithPermissionsAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return null;
                
            return await _context.Roles
                .Include(r => r.RolePermissions.Where(rp => !rp.IsDeleted))
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == roleName && !r.IsDeleted);
        }

        public async Task<PagedResult<Role>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var query = _context.Roles
                .IgnoreQueryFilters()
                .Where(r => r.IsDeleted)
                .Include(r => r.RolePermissions.Where(rp => !rp.IsDeleted))
                    .ThenInclude(rp => rp.Permission)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "name" => parameters.SortDescending
                    ? query.OrderByDescending(r => r.Name)
                    : query.OrderBy(r => r.Name),
                "description" => parameters.SortDescending
                    ? query.OrderByDescending(r => r.Description)
                    : query.OrderBy(r => r.Description),
                "createdat" => parameters.SortDescending
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderBy(r => r.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Role>
            {
                Items = items,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }
    }
}