using System.Collections;
using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly UserServiceDbContext _context;

        public UserRepository(UserServiceDbContext context) : base(context)
        {
            _context = context;
        }
        // In IUserRepository.cs
        // In UserRepository.cs
        async Task<IEnumerable> IUserRepository.GetUserPermissionsAsync(string userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();
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
                .Where(ur => ur.UserId == userId)
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
            return await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.PermissionName == permissionName);
        }

        public async Task<IEnumerable> GetUserPermissionsAsync(string? toString)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == toString)
                .ToListAsync(); 
        }


        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync(); 
        }

        public async Task<IEnumerable<User>> GetDeletedAsync()
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted)
                .ToListAsync(); 
        }

        public async Task<bool> RestoreAsync(string id)
        {
            // First check if user exists (without filter)
            var user = await _context.Users
                .IgnoreQueryFilters()
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
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
    
            // Only mark these properties as modified
            _context.Entry(user).Property(x => x.IsActive).IsModified = true;
            _context.Entry(user).Property(x => x.UpdatedAt).IsModified = true;
    
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User?> GetByIdAsync(string id, bool b)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return false;

            if (user.IsDeleted) return false; // Already deleted

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;    
        }
    }
}
