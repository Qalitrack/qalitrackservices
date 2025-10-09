using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.DTOs.Common;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class RolePermissionRepository(UserServiceDbContext dbContext, ILogger<RolePermissionRepository> logger, IHttpContextAccessor httpContextAccessor)
    : Repository<RolePermission>(dbContext, httpContextAccessor, logger), IRolePermissionRepository
{
    private readonly UserServiceDbContext _context = dbContext;

    public async Task<IEnumerable<RolePermission>> GetAllAsync()
    {
        return await _context.RolePermissions
            .Where(rp => !rp.IsDeleted)
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RolePermission?> GetByIdAsync(string id, bool includeRelated = false)
    {
        var query = _context.RolePermissions
            .Where(rp => !rp.IsDeleted && rp.Id == id);

        if (includeRelated)
        {
            query = query
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission);
        }

        return await query
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await base.DeleteAsync(id);
    }

    public async Task<bool> DeleteByRoleAndPermissionAsync(string roleId, string permissionId)
    {
        var entity = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

        if (entity == null)
            return false;

        var currentUserId = GetCurrentUserId();
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();
        return true;
    }
    

    public async Task AddAsync(RolePermission rolePermission)
    {
        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        rolePermission.Id = Guid.NewGuid().ToString();
        rolePermission.CreatedAt = now;
        rolePermission.UpdatedAt = now;
        rolePermission.CreatedBy = currentUserId;
        rolePermission.UpdatedBy = currentUserId;
        rolePermission.IsDeleted = false;

        await _context.RolePermissions.AddAsync(rolePermission);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<RolePermission>> GetDeletedPagedAsync(PaginationParameters parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var query = _context.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => rp.IsDeleted)
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.ToLower();
            query = query.Where(rp =>
                rp.Role.Name.ToLower().Contains(searchTerm) ||
                rp.Permission.Name.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            switch (parameters.SortBy.ToLower())
            {
                case "role":
                    query = parameters.SortDescending
                        ? query.OrderByDescending(rp => rp.Role.Name)
                        : query.OrderBy(rp => rp.Role.Name);
                    break;
                case "permission":
                    query = parameters.SortDescending
                        ? query.OrderByDescending(rp => rp.Permission.Name)
                        : query.OrderBy(rp => rp.Permission.Name);
                    break;
                case "assignedat":
                    query = parameters.SortDescending
                        ? query.OrderByDescending(rp => rp.AssignedAt)
                        : query.OrderBy(rp => rp.AssignedAt);
                    break;
                default:
                    query = query.OrderBy(rp => rp.AssignedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(rp => rp.AssignedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<RolePermission>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Assigns a permission to a role, handling the case where the relationship was previously soft-deleted
    /// </summary>
    public async Task<bool> AssignOrRestorePermissionToRoleAsync(string roleId, string permissionId)
    {
        // Check if there's an existing record (including soft-deleted ones)
        var existingRecord = await _context.RolePermissions
            .IgnoreQueryFilters() // This allows us to see soft-deleted entities
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        // If we found an existing record
        if (existingRecord != null)
        {
            // If it's already active (not deleted), nothing to do
            if (!existingRecord.IsDeleted)
            {
                return true;
            }
            
            // If it's deleted, restore it
            existingRecord.IsDeleted = false;
            existingRecord.UpdatedAt = now;
            existingRecord.UpdatedBy = currentUserId;
            existingRecord.AssignedAt = now;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        // No existing record, create a new one
        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid().ToString(),
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = currentUserId,
            UpdatedBy = currentUserId,
            IsDeleted = false,
            AssignedAt = now
        };

        await _context.RolePermissions.AddAsync(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }
}