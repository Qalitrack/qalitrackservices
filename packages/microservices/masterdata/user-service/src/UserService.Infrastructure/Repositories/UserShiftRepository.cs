using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserShiftRepository : Repository<UserShift>,IUserShiftRepository
{
    private readonly UserServiceDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserShiftRepository> logger;

    public UserShiftRepository(UserServiceDbContext context) : base(context, new HttpContextAccessor(), new LoggerFactory().CreateLogger<UserShiftRepository>())
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = (IHttpContextAccessor)GetType().GetField("_httpContextAccessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
        logger = (ILogger<UserShiftRepository>)GetType().GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
    }

    public async Task<IEnumerable<UserShift>> GetAllAsync()
    {
        return await _context.UserShifts.ToListAsync();
    }
    

    public async Task<UserShift?> GetByIdAsync(string id, bool b)
    {
        return await _context.UserShifts.FindAsync(id);
    }

    public new  async Task<UserShift> CreateAsync(UserShift userShift)
    {
        if (userShift == null)
            throw new ArgumentNullException(nameof(userShift));
        // Add the userShift to the context using the base class method
        userShift.CreatedAt = DateTime.UtcNow;
        userShift.UpdatedAt = DateTime.UtcNow;
        userShift.CreatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
        var createdUserShift = await base.CreateAsync(userShift);
        await _context.SaveChangesAsync();
        return createdUserShift;
    }

    public async Task<bool> IsUserAssignedToShiftAsync(string userId, string shiftId)
    {
        return await _context.UserShifts
            .AnyAsync(us => us.UserId == userId && us.ShiftId == shiftId);  
    }

    public async Task<IEnumerable<UserShift>> GetShiftsForUserAsync(string? userId, string? shiftId)
    {
        return await _context.UserShifts
            .Where(us => us.UserId == userId && us.ShiftId == shiftId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserShift>> GetUsersAssignedToShiftAsync(string shiftId)
    {
        return await _context.UserShifts
            .Where(us => us.ShiftId == shiftId)
            .Select(us => new UserShift
            {
                UserId = us.UserId,
                ShiftId = us.ShiftId,
                User = new User
                {
                    Id = us.User.Id,
                    Email = us.User.Email,
                    FirstName = us.User.FirstName,
                    LastName = us.User.LastName
                }
            })
            .ToListAsync();
    }
    

    public new async Task<UserShift?> UpdateAsync(UserShift userShift)
    {
        if (userShift == null)
            throw new ArgumentNullException(nameof(userShift));

        var existingUserShift = await _context.UserShifts
            .AsTracking()
            .FirstOrDefaultAsync(us => us.UserId == userShift.UserId && 
                                      us.ShiftId == userShift.ShiftId && 
                                      !us.IsDeleted);

        if (existingUserShift == null)
            return null;

        // Only update specific fields to avoid constraint issues
        existingUserShift.AssignedAt = userShift.AssignedAt;
        existingUserShift.UpdatedAt = DateTime.UtcNow;
        existingUserShift.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

        await _context.SaveChangesAsync();
        return existingUserShift;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var userShift = await _context.UserShifts.FindAsync(id);
        if (userShift == null)
            return false;
        userShift.IsDeleted = true;
        userShift.UpdatedAt = DateTime.UtcNow;
        userShift.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

        await _context.SaveChangesAsync();
        return true;    
    }


    public async Task<bool> DeleteAsync(string userId, string shiftId)
    {
        var userShift = await _context.UserShifts
            .AsTracking()
            .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId);
    
        if (userShift == null)
            return false;

        // Set audit fields before removal
        userShift.IsDeleted = true;
        userShift.UpdatedAt = DateTime.UtcNow;
        userShift.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
    
        await _context.SaveChangesAsync();
        return true;
    }
    
}