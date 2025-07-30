using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.DTOs.Common;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly UserServiceDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserServiceDbContext context, ILogger<UserRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }
        // In IUserRepository.cs
        // In UserRepository.cs
        public async Task<IEnumerable<Permission>>GetUserPermissionsAsync(string userId)
        {
            // Get permissions ONLY from roles (pure RBAC)
            var rolePermissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => !rp.IsDeleted && rp.Role != null && !rp.Role.IsDeleted)
                .Select(rp => rp.Permission)
                .Where(p => !p.IsDeleted)
                .Distinct()
                .ToListAsync();

            return rolePermissions;
        }



        public async Task<IEnumerable> GetUsersByRoleAsync(string roleId)
        {
            return await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<string> roleIds)
        {
            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task<User?> GetByFirstNameAsync(string firstName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.FirstName == firstName);
        }

        public async Task<User?> GetByLastNameAsync(string lastName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.LastName == lastName);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<User?> GetByMobileNumberAsync(string mobileNumber)
        { 
            return await _context.Users.FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);
        }

        public async Task<IEnumerable<UserShift>> GetUserShiftsAsync(string userId)
        {
            return await _context.UserShifts
                .Where(us => us.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserShift?> GetUserShiftByShiftIdAsync(string userId, string shiftId)
        {
            return await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId);   
        }

        public async Task<bool> AssignShiftToUserAsync(string userId, string shiftId)
        {
            return await _context.UserShifts
                .AnyAsync(us => us.UserId == userId && us.ShiftId == shiftId);
        }

        public async Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId)
        {
            return await _context.UserShifts
                .AnyAsync(us => us.UserId == userId && us.ShiftId == shiftId);
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
            user.IsActive = true;  // Ensure user is active after restoration
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
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
                    
                    return await _context.SaveChangesAsync() > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task<User?> GetByIdAsync(string id, bool b)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserShifts)
                    .Include(u => u.UserRoles)
                    .IgnoreQueryFilters()
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null) return false;

                if (user.IsDeleted) return false; // Already deleted

                // Log cascading deletions
                if (user.UserShifts?.Any() == true)
                {
                    var shiftCount = user.UserShifts.Count();
                    _logger.LogInformation("Soft deleting user {UserId} - removing from {ShiftCount} shifts", id, shiftCount);
                    
                    // Remove user from all shifts
                    _context.UserShifts.RemoveRange(user.UserShifts);
                }

                if (user.UserRoles?.Any() == true)
                {
                    var roleCount = user.UserRoles.Count();
                    _logger.LogInformation("Soft deleting user {UserId} - removing {RoleCount} role assignments", id, roleCount);
                    
                    // Remove all role assignments
                    _context.UserRoles.RemoveRange(user.UserRoles);
                }

                // Perform soft delete
                user.IsDeleted = true;
                user.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Successfully soft deleted user {UserId} with cascading cleanup", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error soft deleting user {UserId}", id);
                throw;
            }
        }

        public async Task<PagedResult<User>> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted);

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
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted);

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

            await _context.SaveChangesAsync();
            return existingUser;
        }
    }
}
