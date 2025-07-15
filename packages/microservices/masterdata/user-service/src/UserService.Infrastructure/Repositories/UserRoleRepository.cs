using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        private readonly UserServiceDbContext _context;
        private readonly ILogger<UserRoleRepository> _logger;

        public UserRoleRepository(
            UserServiceDbContext context,
            ILogger<UserRoleRepository> logger) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.UserRoles
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all user roles");
                throw;
            }
        }

        public async Task<UserRole?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));
            }

            try
            {
                return await _context.UserRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ur => ur.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user role with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));
            }

            try
            {
                var rowsAffected = await _context.UserRoles
                    .Where(ur => ur.Id == id)
                    .ExecuteDeleteAsync(cancellationToken);
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user role with ID: {Id}", id);
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
                // Check if the role is already assigned to the user
                var existingAssignment = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
                    
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
                    AssignedAt = DateTime.UtcNow
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
                var rowsAffected = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
                    .ExecuteDeleteAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation(
                        "Successfully removed role {RoleId} from user {UserId}", 
                        roleId, 
                        userId);
                }
                else
                {
                    _logger.LogWarning(
                        "No role {RoleId} found for user {UserId} to remove", 
                        roleId, 
                        userId);
                }

                return rowsAffected > 0;
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
    }
}