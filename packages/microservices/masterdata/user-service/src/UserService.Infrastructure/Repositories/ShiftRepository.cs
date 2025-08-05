using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Utilities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class ShiftRepository(
    UserServiceDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,ILogger<ShiftRepository> logger)
    : Repository<Shift>(dbContext, httpContextAccessor,logger), IShiftRepository
{
    private readonly UserServiceDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

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

    public  async Task<bool> DeleteAsync(string id)
    {
        var shift = await _context.Shifts
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        
        if (shift == null) 
            return false;

        // Perform soft delete
        shift.IsDeleted = true;
        shift.UpdatedAt = DateTime.UtcNow;
        shift.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public new async Task<Shift?> UpdateAsync(Shift shift)
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
        existingShift.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User) ?? "System";

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