using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class ShiftAttendanceRepository : Repository<ShiftAttendance>, IShiftAttendanceRepository
    {
        private readonly UserServiceDbContext _context;
        private readonly ILogger<ShiftAttendanceRepository> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShiftAttendanceRepository(
            UserServiceDbContext context,
            ILogger<ShiftAttendanceRepository> logger,
            IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor, logger)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // Explicit implementation of base interface method
        async Task<IEnumerable<ShiftAttendance>> IRepository<ShiftAttendance>.GetAllAsync()
        {
            return await GetAllAsync(1, 20);
        }

        // Implementation of IShiftAttendanceRepository method with pagination
        public async Task<IEnumerable<ShiftAttendance>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.ShiftAttendances
                .Include(sa => sa.ShiftInstance)
                    .ThenInclude(si => si.Shift)
                .Include(sa => sa.Employee)
                .Where(sa => !sa.IsDeleted)
                .OrderBy(sa => sa.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

       


        public async Task<PagedResult<ShiftAttendance>> GetByInstanceIdAsync(
        string instanceId, 
        int pageNumber = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default)
       {
        try
        {
            _logger.LogInformation("Retrieving attendance records for shift instance {InstanceId} (Page: {PageNumber}, Size: {PageSize})", 
                instanceId, pageNumber, pageSize);

            // Validate pagination
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100); // Cap page size at 100 for performance

            var query = _context.ShiftAttendances
                .AsNoTracking() // Improves read performance
                .Where(sa => sa.ShiftInstanceId == instanceId && !sa.IsDeleted);

            // Get total count first
            var totalCount = await query.CountAsync(cancellationToken);
            
            // Get paginated results with only the required properties
            var items = await query
                .OrderBy(sa => sa.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sa => new ShiftAttendance
                {
                    Id = sa.Id,
                    EmployeeId = sa.EmployeeId,
                    Employee = new User // Only include necessary employee details
                    {
                        Id = sa.Employee.Id,
                        FirstName = sa.Employee.FirstName,
                        LastName = sa.Employee.LastName,
                        Email = sa.Employee.Email,
                    },
                    ClockInTime = sa.ClockInTime,
                    ClockOutTime = sa.ClockOutTime,
                    Status = sa.Status,
                    IsLate = sa.IsLate,
                    IsEarlyDeparture = sa.IsEarlyDeparture,
                    Notes = sa.Notes,
                    CreatedAt = sa.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ShiftAttendance>
            {
                Items = items,
                Page = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
                // HasPreviousPage and HasNextPage are calculated properties
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance records for shift instance {InstanceId}", instanceId);
            throw;
        }
    }

        public Task<ShiftAttendance?> GetByIdAsync(string id, bool b)
        {
            return GetByIdAsync(id);
        }

        public async Task<ShiftAttendance?> GetByIdAsync(string id)
        {
            return await _context.ShiftAttendances
                .Include(sa => sa.ShiftInstance)
                    .ThenInclude(si => si.Shift)
                .Include(sa => sa.Employee)
                .FirstOrDefaultAsync(sa => sa.Id == id && !sa.IsDeleted);
        }

      

        
        public async Task<ShiftAttendance> CreateAsync(ShiftAttendance attendance)
        {
            if (attendance == null)
                throw new ArgumentNullException(nameof(attendance));

            attendance.CreatedAt = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;
            attendance.CreatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
            attendance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

            await _context.ShiftAttendances.AddAsync(attendance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created attendance record for employee {EmployeeId} in shift instance {ShiftInstanceId}",
                attendance.EmployeeId, attendance.ShiftInstanceId);

            return attendance;
        }

        public async Task<ShiftAttendance?> UpdateAsync(ShiftAttendance attendance)
        {
            if (attendance == null)
                throw new ArgumentNullException(nameof(attendance));

            var existingAttendance = await _context.ShiftAttendances
                .AsTracking()
                .FirstOrDefaultAsync(sa => sa.Id == attendance.Id && !sa.IsDeleted);

            if (existingAttendance == null)
            {
                _logger.LogWarning("Attendance record {AttendanceId} not found for update", attendance.Id);
                return null;
            }

            // Update specific fields
            existingAttendance.ClockInTime = attendance.ClockInTime;
            existingAttendance.ClockOutTime = attendance.ClockOutTime;
            existingAttendance.Status = attendance.Status;
            existingAttendance.Notes = attendance.Notes;
            existingAttendance.IsLate = attendance.IsLate;
            existingAttendance.IsEarlyDeparture = attendance.IsEarlyDeparture;
            existingAttendance.UpdatedAt = DateTime.UtcNow;
            existingAttendance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated attendance record {AttendanceId}", attendance.Id);
            return existingAttendance;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var attendance = await _context.ShiftAttendances
                        .AsTracking()
                        .FirstOrDefaultAsync(sa => sa.Id == id);

                    if (attendance == null)
                    {
                        _logger.LogWarning("Attendance record {AttendanceId} not found for deletion", id);
                        return false;
                    }

                    if (attendance.IsDeleted)
                    {
                        _logger.LogWarning("Attendance record {AttendanceId} already marked as deleted", id);
                        return false;
                    }

                    attendance.IsDeleted = true;
                    attendance.UpdatedAt = DateTime.UtcNow;
                    attendance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Soft deleted attendance record {AttendanceId}", id);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting attendance record {AttendanceId}", id);
                    throw;
                }
            });
        }

        public async Task<ShiftAttendance?> ClockInAsync(string shiftInstanceId, string employeeId, DateTime clockInTime, string? notes = null)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Check if already clocked in
                    var existingAttendance = await _context.ShiftAttendances
                        .FirstOrDefaultAsync(sa => sa.ShiftInstanceId == shiftInstanceId &&
                                                   sa.EmployeeId == employeeId &&
                                                   !sa.IsDeleted);

                    if (existingAttendance != null)
                    {
                        if (existingAttendance.ClockInTime.HasValue)
                        {
                            _logger.LogWarning("Employee {EmployeeId} already clocked in for shift instance {ShiftInstanceId}",
                                employeeId, shiftInstanceId);
                            return null; // Already clocked in
                        }

                        // Update existing record with clock-in time
                        existingAttendance.ClockInTime = clockInTime;
                        existingAttendance.Status = AttendanceStatus.Present;
                        existingAttendance.Notes = notes;
                        existingAttendance.UpdatedAt = DateTime.UtcNow;
                        existingAttendance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                        // Check if late
                        var shiftInstance = await _context.ShiftInstances
                            .FirstOrDefaultAsync(si => si.Id == shiftInstanceId);

                        if (shiftInstance != null)
                        {
                            existingAttendance.IsLate = clockInTime > shiftInstance.ScheduledStartTime;
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Employee {EmployeeId} clocked in for shift instance {ShiftInstanceId}",
                            employeeId, shiftInstanceId);
                        return existingAttendance;
                    }

                    // Create new attendance record
                    var newAttendance = new ShiftAttendance
                    {
                        Id = Guid.NewGuid().ToString(),
                        ShiftInstanceId = shiftInstanceId,
                        EmployeeId = employeeId,
                        ClockInTime = clockInTime,
                        Status = AttendanceStatus.Present,
                        Notes = notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User),
                        UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User)
                    };

                    // Check if late
                    var shift = await _context.ShiftInstances
                        .FirstOrDefaultAsync(si => si.Id == shiftInstanceId);

                    if (shift != null)
                    {
                        newAttendance.IsLate = clockInTime > shift.ScheduledStartTime;
                    }

                    await _context.ShiftAttendances.AddAsync(newAttendance);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Created new attendance and clocked in employee {EmployeeId} for shift instance {ShiftInstanceId}",
                        employeeId, shiftInstanceId);
                    return newAttendance;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during clock-in for employee {EmployeeId} in shift instance {ShiftInstanceId}",
                        employeeId, shiftInstanceId);
                    throw;
                }
            });
        }

        public async Task<ShiftAttendance?> ClockOutAsync(string shiftInstanceId, string employeeId, DateTime clockOutTime, string? notes = null)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var attendance = await _context.ShiftAttendances
                        .Include(sa => sa.ShiftInstance)
                        .AsTracking()
                        .FirstOrDefaultAsync(sa => sa.ShiftInstanceId == shiftInstanceId &&
                                                   sa.EmployeeId == employeeId &&
                                                   !sa.IsDeleted);

                    if (attendance == null)
                    {
                        _logger.LogWarning("No attendance record found for employee {EmployeeId} in shift instance {ShiftInstanceId}",
                            employeeId, shiftInstanceId);
                        return null;
                    }

                    if (!attendance.ClockInTime.HasValue)
                    {
                        _logger.LogWarning("Employee {EmployeeId} trying to clock out without clocking in for shift instance {ShiftInstanceId}",
                            employeeId, shiftInstanceId);
                        return null;
                    }

                    if (attendance.ClockOutTime.HasValue)
                    {
                        _logger.LogWarning("Employee {EmployeeId} already clocked out for shift instance {ShiftInstanceId}",
                            employeeId, shiftInstanceId);
                        return null;
                    }

                    attendance.ClockOutTime = clockOutTime;
                    attendance.Notes = string.IsNullOrEmpty(notes) ? attendance.Notes :
                        string.IsNullOrEmpty(attendance.Notes) ? notes : $"{attendance.Notes}; {notes}";
                    attendance.UpdatedAt = DateTime.UtcNow;
                    attendance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                    // Check if early departure
                    if (attendance.ShiftInstance != null)
                    {
                        attendance.IsEarlyDeparture = clockOutTime < attendance.ShiftInstance.ScheduledEndTime;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Employee {EmployeeId} clocked out for shift instance {ShiftInstanceId}",
                        employeeId, shiftInstanceId);
                    return attendance;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during clock-out for employee {EmployeeId} in shift instance {ShiftInstanceId}",
                        employeeId, shiftInstanceId);
                    throw;
                }
            });
        }
        

        // New DTO-returning methods
        public async Task<IEnumerable<ShiftAttendanceResponse>> GetShiftAttendancesForInstanceAsync(string shiftInstanceId)
        {
            return await _context.ShiftAttendances
                .Where(sa => sa.ShiftInstanceId == shiftInstanceId && !sa.IsDeleted)
                .Join(_context.Users,
                    attendance => attendance.EmployeeId,
                    user => user.Id,
                    (attendance, user) => new { attendance, user })
                .Select(x => new ShiftAttendanceResponse
                {
                    Id = x.attendance.Id,
                    EmployeeId = x.attendance.EmployeeId,
                    EmployeeName = $"{x.user.FirstName} {x.user.LastName}",
                    EmployeeEmail = x.user.Email,
                    ClockInTime = x.attendance.ClockInTime,
                    ClockOutTime = x.attendance.ClockOutTime,
                    Status = x.attendance.Status,
                    Notes = x.attendance.Notes,
                    IsLate = x.attendance.IsLate,
                    CreatedAt = x.attendance.CreatedAt,
                    UpdatedAt = x.attendance.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftAttendanceResponse>> GetEmployeeAttendancesAsync(string employeeId)
        {
            return await _context.ShiftAttendances
                .Where(sa => sa.EmployeeId == employeeId && !sa.IsDeleted)
                .Join(_context.Users,
                    attendance => attendance.EmployeeId,
                    user => user.Id,
                    (attendance, user) => new { attendance, user })
                .Select(x => new ShiftAttendanceResponse
                {
                    Id = x.attendance.Id,
                    EmployeeId = x.attendance.EmployeeId,
                    EmployeeName = $"{x.user.FirstName} {x.user.LastName}",
                    EmployeeEmail = x.user.Email,
                    ClockInTime = x.attendance.ClockInTime,
                    ClockOutTime = x.attendance.ClockOutTime,
                    Status = x.attendance.Status,
                    IsLate = x.attendance.IsLate,
                    CreatedAt = x.attendance.CreatedAt,
                    UpdatedAt = x.attendance.UpdatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ShiftAttendanceResponse?> GetAttendanceDetailsAsync(string id)
        {
            var query = from attendance in _context.ShiftAttendances
                        join user in _context.Users on attendance.EmployeeId equals user.Id
                        where attendance.Id == id && !attendance.IsDeleted
                        select new ShiftAttendanceResponse
                        {
                            Id = attendance.Id,
                            EmployeeId = attendance.EmployeeId,
                            EmployeeName = $"{user.FirstName} {user.LastName}",
                            EmployeeEmail = user.Email,
                            ClockInTime = attendance.ClockInTime,
                            ClockOutTime = attendance.ClockOutTime,
                            Status = attendance.Status,
                            IsLate = attendance.IsLate,
                            CreatedAt = attendance.CreatedAt,
                            UpdatedAt = attendance.UpdatedAt
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ShiftAttendance?> GetByUserAndInstanceAsync(string userId, string shiftInstanceId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(shiftInstanceId))
                {
                    _logger.LogWarning("GetByUserAndInstanceAsync called with null or empty parameters. UserId: {UserId}, ShiftInstanceId: {ShiftInstanceId}", 
                        userId, shiftInstanceId);
                    return null;
                }

                return await _context.ShiftAttendances
                    .Include(sa => sa.Employee)
                    .Include(sa => sa.ShiftInstance)
                        .ThenInclude(si => si.Shift)
                    .FirstOrDefaultAsync(sa => 
                        sa.EmployeeId == userId && 
                        sa.ShiftInstanceId == shiftInstanceId && 
                        !sa.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByUserAndInstanceAsync for UserId: {UserId}, ShiftInstanceId: {ShiftInstanceId}", 
                    userId, shiftInstanceId);
                throw;
            }
        }
        
    }
}