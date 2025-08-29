using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Core.DTOs.Common;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserShiftRepository : Repository<UserShift>, IUserShiftRepository
{
    private readonly UserServiceDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly new ILogger<UserShiftRepository> _logger;

    public UserShiftRepository(
        UserServiceDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserShiftRepository> logger
    ) : base(context, httpContextAccessor, logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<IEnumerable<UserShift>> GetAllAsync()
    {
        return await _context.UserShifts
            .Include(us => us.User)
            .Include(us => us.Shift)
            .Where(us => !us.IsDeleted)
            .ToListAsync();
    }
    

    public async Task<UserShift?> GetByIdAsync(string id, bool b)
    {
        return await _context.UserShifts
            .Include(us => us.User)
            .Include(us => us.Shift)
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);
    }

    public new  async Task<UserShift> CreateAsync(UserShift userShift)
    {
        if (userShift == null)
            throw new ArgumentNullException(nameof(userShift));
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
            .AnyAsync(us => us.UserId == userId && us.ShiftId == shiftId && !us.IsDeleted);  
    }

    public async Task<IEnumerable<UserShift>> GetShiftsForUserAsync(string? userId, string? shiftId)
    {
        var query = _context.UserShifts
            .Include(us => us.Shift)
            .Where(us => !us.IsDeleted && us.Shift != null && !us.Shift.IsDeleted);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(us => us.UserId == userId);

        if (!string.IsNullOrEmpty(shiftId))
            query = query.Where(us => us.ShiftId == shiftId);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<UserShift>> GetUsersAssignedToShiftAsync(string shiftId)
    {
        return await _context.UserShifts
            .Where(us => us.ShiftId == shiftId && !us.IsDeleted && us.User != null && !us.User.IsDeleted)
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
        var userShift = await _context.UserShifts
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);
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
    
    public async Task<PagedResult<UserShift>> GetDeletedPagedAsync(PaginationParameters parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var query = _context.UserShifts
            .Include(us => us.User)
            .Include(us => us.Shift)
            .Where(us => us.IsDeleted)
            .IgnoreQueryFilters();

        // Apply search if provided
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(us => 
                (us.User != null && us.User.Email.Contains(parameters.Search)) ||
                (us.Shift != null && us.Shift.Name.Contains(parameters.Search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(us => us.UpdatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<UserShift>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }
    
}