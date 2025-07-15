using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserShiftRepository : Repository<UserShift>,IUserShiftRepository
{
    private readonly UserServiceDbContext _context;

    public UserShiftRepository(UserServiceDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<UserShift>> GetAllAsync()
    {
        return await _context.UserShifts.ToListAsync();
    }

    public async Task<UserShift?> GetByIdAsync(string id)
    {
        return await _context.UserShifts.FindAsync(id);
    }

    public async Task<UserShift> CreateAsync(UserShift userShift)
    {
        if (userShift == null)
            throw new ArgumentNullException(nameof(userShift));

        _context.UserShifts.Add(userShift);
        await _context.SaveChangesAsync();
        return userShift;
    }

    public async Task<UserShift?> UpdateAsync(UserShift userShift)
    {
        if (userShift == null)
            throw new ArgumentNullException(nameof(userShift));

        _context.Entry(userShift).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return userShift;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var userShift = await _context.UserShifts.FindAsync(id);
        if (userShift == null)
            return false;

        _context.UserShifts.Remove(userShift);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignUserToShiftAsync(string userId, string shiftId)
    {
        // Check if the assignment already exists
        var exists = await _context.UserShifts
            .AnyAsync(us => us.UserId == userId && us.ShiftId == shiftId);
            
        if (exists)
            return false;

        var userShift = new UserShift
        {
            UserId = userId,
            ShiftId = shiftId,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserShifts.Add(userShift);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId)
    {
        var userShift = await _context.UserShifts
            .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId);
            
        if (userShift == null)
            return false;

        _context.UserShifts.Remove(userShift);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserShift>> GetShiftsForUserAsync(string userId)
    {
        return await _context.UserShifts
            .Where(us => us.UserId == userId)
            .ToListAsync();
    }
}