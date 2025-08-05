using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
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
        private readonly ILogger<UserRoleRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                
                _logger.LogInformation("Successfully assigned role {RoleId} to user {UserId}", roleId, userId);
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
                _logger.LogInformation("Attempting to remove role {RoleId} from user {UserId}", roleId, userId);
                
                // First check if the role assignment exists
                var existingAssignment = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAssignment == null)
                {
                    _logger.LogWarning(
                        "No active role {RoleId} found for user {UserId} to remove - role may already be removed", 
                        roleId, 
                        userId);
                    return false;
                }

                _logger.LogInformation("Found existing assignment with ID {AssignmentId} for user {UserId} and role {RoleId}", 
                    existingAssignment.Id, userId, roleId);

                // Hard delete the user role assignment to completely break the association
                var rowsAffected = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted)
                    .ExecuteDeleteAsync(cancellationToken);

                _logger.LogInformation("ExecuteDeleteAsync affected {RowsAffected} rows for user {UserId} and role {RoleId}", 
                    rowsAffected, userId, roleId);

                if (rowsAffected == 0)
                {
                    _logger.LogError(
                        "ExecuteDeleteAsync returned 0 rows affected when removing role {RoleId} from user {UserId} - this should not happen", 
                        roleId, 
                        userId);
                    return false;
                }

                // Verify the deletion worked
                var verifyDeletion = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);

                if (verifyDeletion)
                {
                    _logger.LogError(
                        "Verification failed: Role {RoleId} still exists for user {UserId} after deletion attempt", 
                        roleId, 
                        userId);
                    return false;
                }

                _logger.LogInformation(
                    "Successfully removed role {RoleId} from user {UserId} - verified deletion", 
                    roleId, 
                    userId);

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
    }
}