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
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
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

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();  
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;    
        }
    }
}
