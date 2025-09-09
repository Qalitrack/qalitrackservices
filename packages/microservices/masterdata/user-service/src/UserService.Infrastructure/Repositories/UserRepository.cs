using System.Collections;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository(
        UserServiceDbContext context,
        ILogger<UserRepository> logger,
        IHttpContextAccessor httpContextAccessor)
        : Repository<User>(context, httpContextAccessor, logger), IUserRepository
    {
        private readonly UserServiceDbContext _context = context;
        public async Task<IEnumerable<Permission>>GetUserPermissionsAsync(string userId)
        {
            var rolePermissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => !rp.IsDeleted  && !rp.Role.IsDeleted)
                .Select(rp => rp.Permission)
                .Where(p => !p.IsDeleted)
                .Distinct()
                .ToListAsync();

            return rolePermissions;
        }



        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleId)
        {
            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId && !ur.IsDeleted)
                .Select(ur => ur.User)
                .Where(u => u != null && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .ToListAsync();
        }

     
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }


        public async Task<IEnumerable<UserShift>> GetUserShiftsAsync(string userId)
        {
            return await _context.UserShifts
                .Where(us => us.UserId == userId && !us.IsDeleted)
                .ToListAsync();
        }

        public async Task<UserShift?> GetUserShiftByShiftIdAsync(string userId, string shiftId)
        {
            return await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId && !us.IsDeleted);   
        }

        public async Task<bool> AssignShiftToUserAsync(string userId, string shiftId)
        {
            // Check if there's an existing relationship (active)
            var existingAssignment = await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId && !us.IsDeleted);
                
            if (existingAssignment != null)
            {
                // Assignment already exists and is active
                logger.LogDebug("User {UserId} is already assigned to shift {ShiftId}", userId, shiftId);
                return true;
            }
            
            // Check if there's a soft-deleted relationship
            var softDeletedAssignment = await _context.UserShifts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId && us.IsDeleted);
        
            if (softDeletedAssignment != null)
            {
                // Restore the soft-deleted relationship
                softDeletedAssignment.IsDeleted = false;
                softDeletedAssignment.UpdatedAt = DateTime.UtcNow;
                softDeletedAssignment.UpdatedBy = GetCurrentUserId();
                softDeletedAssignment.AssignedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                logger.LogInformation("Restored previously deleted assignment of user {UserId} to shift {ShiftId}", userId, shiftId);
                return true;
            }
            
            // Create a new assignment
            var userShift = new UserShift
            {
                UserId = userId,
                ShiftId = shiftId,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId(),
                UpdatedBy = GetCurrentUserId()
            };
            
            await _context.UserShifts.AddAsync(userShift);
            await _context.SaveChangesAsync();
            logger.LogInformation("Assigned user {UserId} to shift {ShiftId}", userId, shiftId);
            return true;
        }

        public async Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId)
        {
            // Find the existing assignment
            var assignment = await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId && !us.IsDeleted);
                
            if (assignment == null)
            {
                // Assignment doesn't exist or is already soft-deleted
                logger.LogDebug("No active assignment found for user {UserId} and shift {ShiftId}", userId, shiftId);
                return false;
            }
            
            // Soft-delete the assignment
            assignment.IsDeleted = true;
            assignment.UpdatedAt = DateTime.UtcNow;
            assignment.UpdatedBy = GetCurrentUserId();
            
            await _context.SaveChangesAsync();
            logger.LogInformation("Removed shift {ShiftId} assignment from user {UserId}", shiftId, userId);
            return true;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionName)
        {
            // Check permissions through roles only (pure RBAC)
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => !rp.IsDeleted)
                .Select(rp => rp.Permission)
                .Where(p => !p.IsDeleted)
                .AnyAsync(p => p.Name == permissionName);
        }
        

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted)
                .ToListAsync(); 
        }

        public async Task<bool> RestoreAsync(string id)
        {
            // Create execution strategy from the context
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // First check if user exists (without filter)
                    var user = await _context.Users
                        .IgnoreQueryFilters()
                        .AsTracking()
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (user == null)
                    {
                        return false; // User doesn't exist
                    }

                    if (!user.IsDeleted)
                    {
                        return false; // User is already active
                    }

                    // Check if password hash is valid
                    if (string.IsNullOrWhiteSpace(user.Password) || !user.Password.StartsWith("$2a$"))
                    {
                        // If password hash is invalid, set a default password that needs to be changed
                        user.Password = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!");
                        user.IsFirstLogin = true;
                    }

                    user.IsDeleted = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = AuthUtils.GetUserIdFromClaims(HttpContextAccessor.HttpContext?.User);

                    logger.LogInformation("[UserRepository] Set IsDeleted=false for user {UserId}", id);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Error restoring user {UserId}", id);
                    throw;
                }
            });
        }

        public async Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive)
        {
            try
            {
                // Try using raw SQL first as a more direct approach
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Users\" SET \"IsActive\" = {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
                    isActive, DateTime.UtcNow, userId);
                
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                try
                {
                    // Fallback to EF approach
                    var user = await _context.Users
                        .AsTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user == null) 
                    {
                        return false;
                    }
                    
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = AuthUtils.GetUserIdFromClaims(HttpContextAccessor.HttpContext?.User);
                    
                    
                    return await _context.SaveChangesAsync() > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task<User?> GetByIdAsync(string id, bool includeRoles = true)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (includeRoles)
                {
                    query = query
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role);
                }

                var user = await query.Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

                if (user == null)
                {
                    return null;
                }

                if (includeRoles)
                {
                    // Use pattern matching for cleaner null checks
                    if (user.UserRoles is { Count: > 0 } userRoles)
                    {
                        foreach (var userRole in userRoles)
                        {
                            // Use null-conditional operator and pattern matching
                            var roleInfo = userRole.Role is { } role 
                                ? $"RoleId: {userRole.RoleId}, RoleName: {role.Name}" 
                                : $"RoleId: {userRole.RoleId}, Role is null";
                        }
                    }
                }

                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error in GetByIdAsync for UserId: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            // Create execution strategy from the context
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users
                        .IgnoreQueryFilters()
                        .AsTracking()
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (user == null) 
                    {
                        logger.LogWarning("[UserRepository] User {UserId} not found for deletion", id);
                        return false;
                    }
                    
                    if (user.IsDeleted) 
                    {
                        logger.LogWarning("[UserRepository] User {UserId} already marked as deleted", id);
                        return false; // Already deleted
                    }

                    user.IsDeleted = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = AuthUtils.GetUserIdFromClaims(HttpContextAccessor.HttpContext?.User);

                    logger.LogInformation("[UserRepository] Set IsDeleted=true for user {UserId}", id);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Error deleting user {UserId}", id);
                    throw;
                }
            });
        }

        public async Task<PagedResult<User>> GetPagedAsync(PaginationParameters parameters)
        {
            // This query will only return users where IsDeleted is false
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.IsDeleted == false); // Explicitly check for false

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.MobileNumber.Contains(searchTerm));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "firstname" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.FirstName)
                        : query.OrderBy(u => u.FirstName),
                    "lastname" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.LastName)
                        : query.OrderBy(u => u.LastName),
                    "email" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email),
                    "createdat" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.CreatedAt)
                        : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderBy(u => u.FirstName)
                };
            }
            else
            {
                query = query.OrderBy(u => u.FirstName);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResult<User>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            // This query will only return users where IsDeleted is true
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted == true); // Explicitly check for true

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.MobileNumber.Contains(searchTerm));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "firstname" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.FirstName)
                        : query.OrderBy(u => u.FirstName),
                    "lastname" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.LastName)
                        : query.OrderBy(u => u.LastName),
                    "email" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email),
                    "createdat" => parameters.SortDescending 
                        ? query.OrderByDescending(u => u.CreatedAt)
                        : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderBy(u => u.FirstName)
                };
            }
            else
            {
                query = query.OrderBy(u => u.FirstName);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        public override async Task<User?> UpdateAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existingUser = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Id == entity.Id && !u.IsDeleted);

            if (existingUser == null)
                return null;

            // Only update specific fields to avoid constraint issues
            existingUser.FirstName = entity.FirstName;
            existingUser.LastName = entity.LastName;
            existingUser.Email = entity.Email;
            existingUser.MobileNumber = entity.MobileNumber;
            existingUser.Password = entity.Password;
            existingUser.IsActive = entity.IsActive;
            existingUser.IsFirstLogin = entity.IsFirstLogin;
            existingUser.UpdatedAt = DateTime.UtcNow;
            existingUser.UpdatedBy = AuthUtils.GetUserIdFromClaims(HttpContextAccessor.HttpContext?.User);
            await _context.SaveChangesAsync();
            return existingUser;
        }
        
        // Function 1: Get user roles by user ID
public async Task<IEnumerable<Role>> GetUserRolesByUserIdAsync(string userId)
{
    if (string.IsNullOrWhiteSpace(userId))
        return new List<Role>();
        
    return await _context.UserRoles
        .Where(ur => ur.UserId == userId && !ur.IsDeleted)
        .Include(ur => ur.Role)
        .Select(ur => ur.Role)
        .Where(r => r != null && !r.IsDeleted)
        .ToListAsync();
}

    // Function 2: Reset user password and send email
    public async Task<bool> ResetUserPasswordAsync(string userId, IEmailQueueService emailQueueService)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required", nameof(userId));
            
        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            
        if (user == null)
            return false;
            
        // Set default password and mark as first login
        const string defaultPassword = "ChangeMe123!";
        user.Password = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
        user.IsFirstLogin = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = AuthUtils.GetUserIdFromClaims(HttpContextAccessor.HttpContext?.User);
        
        await _context.SaveChangesAsync();
        
        // Send email with new password
        try
        {
            var emailSubject = "Password Reset - Please Change Your Password";
            var emailBody = $@"
                Dear {user.FirstName} {user.LastName},
                
                Your password has been reset. Please use the following temporary password to log in:
                
                Password: {defaultPassword}
                
                For security reasons, you will be required to change this password upon your next login.
                
                If you did not request this password reset, please contact your system administrator immediately.
                
                Best regards,
                System Administrator";
                
            await emailQueueService.EnqueueEmailAsync(user.Email, emailSubject, emailBody);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue password reset email for user {UserId}", userId);
            return true;
        }
      }
    }
}
