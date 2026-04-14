using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class ShiftRepository(
    UserServiceDbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,ILogger<ShiftRepository> logger)
    : Repository<Shift>(dbContext, httpContextAccessor,logger), IShiftRepository
{
    private readonly UserServiceDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    
    // Explicit implementation of base interface method
    async Task<IEnumerable<Shift>> IRepository<Shift>.GetAllAsync()
    {
        return await GetAllAsync(1, 10);
    }

    // Implementation of IShiftRepository method with pagination
    public async Task<IEnumerable<Shift>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var shifts = await _context.Shifts
            .Include(s => s.UserShifts.Where(us => !us.IsDeleted && us.User != null && !us.User.IsDeleted))
            .ThenInclude(us => us.User)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return shifts;
    }

    public async Task<Shift?> GetByIdAsync(string id, bool includeDeleted = false)
    {
        var query = _context.Shifts.AsQueryable();
    
        if (includeDeleted)
        {
            return await query
                .Include(s => s.UserShifts)
                .ThenInclude(us => us.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        else
        {
            return await query
                .Include(s => s.UserShifts.Where(us => !us.IsDeleted && us.User != null && !us.User.IsDeleted))
                .ThenInclude(us => us.User)
                .Where(s => !s.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }

    public async Task<Shift?> GetByIdAsync(string id)
    {
        return await _context.Shifts
            .Include(s => s.UserShifts.Where(us => !us.IsDeleted && us.User != null && !us.User.IsDeleted))
            .ThenInclude(us => us.User)
            .Where(s => s.Id == id && !s.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync();
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
    
   public async Task<Shift?> UpdateEnhancedAsync(UpdateShiftRequest request, string updatedBy)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingShift = await _context.Shifts
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted);

            if (existingShift == null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Validate business rules

            if (request.StartTime >= request.EndTime)
            {
                throw new ValidationException("End time must be after start time");
            }

            if (request.EndDate.HasValue && request.EndDate < request.StartDate)
            {
                throw new ValidationException("End date cannot be before start date");
            }

            // Update shift properties
            existingShift.Name = request.Name;
            existingShift.Description = request.Description;
            existingShift.StartTime = request.StartTime;
            existingShift.EndTime = request.EndTime;
            existingShift.Mode = request.Mode;
            existingShift.StartDate = request.StartDate;
            existingShift.EndDate = request.EndDate;
            existingShift.Status = existingShift.Status;
            existingShift.RequiredStaffCount = request.RequiredStaffCount;
            existingShift.RecurrenceType = request.RecurrenceType;
            existingShift.RecurrenceInterval = request.RecurrenceInterval;
            existingShift.CustomDays = request.CustomDays ?? Array.Empty<DayOfWeek>();
            existingShift.ExceptionDates = request.ExceptionDates ?? Array.Empty<DateTime>();
            existingShift.UpdatedAt = DateTime.UtcNow;
            existingShift.UpdatedBy = updatedBy;

            // Update in database
            _context.Shifts.Update(existingShift);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            return existingShift;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    

    public async Task<bool> IsShiftActiveAsync(string shiftId)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == shiftId && !s.IsDeleted);
        if (shift == null)
            return false;

        return shift.IsActive;
    }

    public async Task<PagedResult<Shift>> GetDeletedPagedAsync(PaginationParameters parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var query = _context.Shifts
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchTerm) ||
                s.Description.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        switch (parameters.SortBy?.ToLower())
        {
            case "name":
                query = parameters.SortDescending
                    ? query.OrderByDescending(s => s.Name)
                    : query.OrderBy(s => s.Name);
                break;
            case "description":
                query = parameters.SortDescending
                    ? query.OrderByDescending(s => s.Description)
                    : query.OrderBy(s => s.Description);
                break;
            case "createdat":
                query = parameters.SortDescending
                    ? query.OrderByDescending(s => s.CreatedAt)
                    : query.OrderBy(s => s.CreatedAt);
                break;
            default:
                query = query.OrderBy(s => s.Name);
                break;
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Shift>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Shifts
            .Where(s => !s.IsDeleted && 
                       s.Status == ShiftStatus.Published &&
                       (s.StartDate <= endDate) && 
                       (!s.EndDate.HasValue || s.EndDate >= startDate))
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Shifts
            .Where(s => !s.IsDeleted)
            .CountAsync();
    }

    public async Task<IEnumerable<Shift>> GetActiveRecurringShiftsAsync()
    {
        var today = DateTime.Today;
        
        return await _context.Shifts
            .Where(s => !s.IsDeleted &&
                       s.Status == ShiftStatus.Published &&
                       s.Type == ShiftType.Recurring &&
                       (s.EndDate == null || s.EndDate >= today))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetShiftsByStatusAsync(ShiftStatus status)
    {
        return await _context.Shifts
            .Where(s => !s.IsDeleted && s.Status == status)
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetShiftsByFilterAsync(ShiftFilterRequest filter)
    {
        var query = _context.Shifts
            .Where(s => !s.IsDeleted);
            
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim().ToLower();
            query = query.Where(s => 
                s.Name.ToLower().Contains(searchTerm) ||
                s.Description.ToLower().Contains(searchTerm));
        }
        
        if (filter.Status.HasValue)
        {
            query = query.Where(s => s.Status == filter.Status.Value);
        }
        
        // Add date range filtering if provided
        if (filter.StartDate.HasValue)
        {
            query = query.Where(s => s.StartDate >= filter.StartDate.Value);
        }
        
        if (filter.EndDate.HasValue)
        {
            query = query.Where(s => !s.EndDate.HasValue || s.EndDate <= filter.EndDate.Value);
        }
        
        // Add sorting
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            query = filter.SortBy.ToLower() switch
            {
                "name" when filter.SortDescending == true => query.OrderByDescending(s => s.Name),
                "name" => query.OrderBy(s => s.Name),
                "startdate" when filter.SortDescending == true => query.OrderByDescending(s => s.StartDate),
                "startdate" => query.OrderBy(s => s.StartDate),
                _ => query.OrderBy(s => s.StartDate).ThenBy(s => s.StartTime)
            };
        }
        else
        {
            query = query.OrderBy(s => s.StartDate).ThenBy(s => s.StartTime);
        }
        
        // Apply pagination
        if (filter.PageSize > 0 && filter.Page > 0)
        {
            query = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }
        
        return await query.ToListAsync();
    }
}