using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class ShiftRepository : Repository<Shift>, IShiftRepository
{
    private readonly UserServiceDbContext _context;
    
    public ShiftRepository(UserServiceDbContext dbContext) : base(dbContext)
    {
        _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public new async Task<IEnumerable<Shift>> GetAllAsync()
    {
        return await _context.Shifts.ToListAsync();
    }

    public async Task<Shift?> GetByIdAsync(string id)
    {
        return await _context.Shifts.FirstOrDefaultAsync(s => s.Id == id);
    }

    public new async Task<Shift?> GetByIdAsync(string id, bool b)
    {
        return await _context.Shifts.FirstOrDefaultAsync(s => s.Id == id);
    }

    public new async Task<bool> DeleteAsync(string id)
    {
        var shift = await _context.Shifts
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        if (shift == null) return false;

        _context.Shifts.Remove(shift);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Shift> CreateAsync(Shift shift)
    {
        if (shift == null)
            throw new ArgumentNullException(nameof(shift));

        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();
        return shift;
    }

    public async Task<Shift?> UpdateAsync(Shift shift)
    {
        if (shift == null)
            throw new ArgumentNullException(nameof(shift));

        var existingShift = await _context.Shifts
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == shift.Id && !s.IsDeleted);

        if (existingShift == null)
            return null;

        // Only update specific fields to avoid constraint issues
        existingShift.Name = shift.Name;
        existingShift.Description = shift.Description;
        existingShift.StartTime = shift.StartTime;
        existingShift.EndTime = shift.EndTime;
        existingShift.Mode = shift.Mode;
        existingShift.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingShift;
    }
    

    public async Task<bool> IsShiftActiveAsync(string shiftId)
    {
        var shift = await _context.Shifts.FindAsync(shiftId);
        if (shift == null)
            return false;

        return shift.IsActive;
    }

   
}