using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.DTOs.Common;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRoleRepository(
        UserServiceDbContext context,
        ILogger<UserRoleRepository> logger,
        IHttpContextAccessor httpContextAccessor)
        : Repository<UserRole>(context, httpContextAccessor, logger), IUserRoleRepository
    {
        private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private new readonly ILogger<UserRoleRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<IEnumerable<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Include(ur => ur.User)
                    .Where(ur => !ur.IsDeleted)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all user roles");
                throw;
            }
        }

       

        public async Task<bool> AssignRoleToUserAsync(
            string userId, 
            string roleId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or whitespace", nameof(roleId));

            try
            {
                // Check if the role is already assigned to the user (exclude soft-deleted)
                var existingAssignment = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);
                    
                if (existingAssignment)
                {
                    _logger.LogInformation("User {UserId} already has role {RoleId} assigned", userId, roleId);
                    return true;
                }

                // Create and add new UserRole
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId(),
                    UpdatedBy = GetCurrentUserId(),
                    IsDeleted = false,
                    
                };
                
                await _context.UserRoles.AddAsync(userRole, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error occurred while assigning role {RoleId} to user {UserId}", 
                    roleId, 
                    userId);
                throw;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(
            string userId, 
            string roleId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or whitespace", nameof(roleId));

            try
            {
                
                // First check if the role assignment exists
                var existingAssignment = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAssignment == null)
                {
                    return false;
                }
                

                // Hard delete the user role assignment to completely break the association
                var rowsAffected = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted)
                    .ExecuteDeleteAsync(cancellationToken);
                

                if (rowsAffected == 0)
                {
                    return false;
                }

                // Verify the deletion worked
                var verifyDeletion = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);

                if (verifyDeletion)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error occurred while removing role {RoleId} from user {UserId}", 
                    roleId, 
                    userId);
                throw;
            }
        }

        public async Task<int> RemoveRoleFromAllUsersAsync(string roleId)
        {
            var currentUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
    
            // Get all user-role assignments for this role
            var userRoles = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId && !ur.IsDeleted)
                .ToListAsync();

            var count = userRoles.Count;
            if (count == 0)
                return 0;

            // Update all in memory
            foreach (var userRole in userRoles)
            {
                userRole.IsDeleted = true;
                userRole.UpdatedAt = now;
                userRole.UpdatedBy = currentUserId;
            }

            await _context.SaveChangesAsync();
            return count;
        }

        public async Task<PagedResult<UserRole>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            var query = _context.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.IsDeleted)
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLower();
                query = query.Where(ur => 
                    ur.UserId.ToLower().Contains(searchTerm) ||
                    ur.RoleId.ToLower().Contains(searchTerm) ||
                    (ur.User != null && ur.User.Email.ToLower().Contains(searchTerm)) ||
                    (ur.Role != null && ur.Role.Name.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "userid" => parameters.SortDescending 
                        ? query.OrderByDescending(ur => ur.UserId)
                        : query.OrderBy(ur => ur.UserId),
                    "roleid" => parameters.SortDescending 
                        ? query.OrderByDescending(ur => ur.RoleId)
                        : query.OrderBy(ur => ur.RoleId),
                    "assignedat" => parameters.SortDescending 
                        ? query.OrderByDescending(ur => ur.AssignedAt)
                        : query.OrderBy(ur => ur.AssignedAt),
                    "createdat" => parameters.SortDescending 
                        ? query.OrderByDescending(ur => ur.CreatedAt)
                        : query.OrderBy(ur => ur.CreatedAt),
                    _ => query.OrderBy(ur => ur.AssignedAt)
                };
            }
            else
            {
                query = query.OrderBy(ur => ur.AssignedAt);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<UserRole>
            {
                Items = items,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }
    }
}