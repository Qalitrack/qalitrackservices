using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Report;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly UserServiceDbContext _context;
        private readonly ILogger<ReportService> _logger;
        private readonly IMapper _mapper;

        public ReportService(
            UserServiceDbContext context,
            ILogger<ReportService> logger,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ShiftReportResponse> GenerateShiftReportAsync()
        {
            try
            {
                var shifts = await _context.Shifts
                    .Include(s => s.UserShifts)
                    .ThenInclude(us => us.User)
                    .AsNoTracking()
                    .ToListAsync();

                var shiftReports = shifts.Select(s => new ShiftReportDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Mode = s.Mode.ToString(),
                    IsActive = s.IsActive,
                    AssignedUsersCount = s.UserShifts?.Count ?? 0,
                    LastModified = s.UpdatedAt > s.CreatedAt ? s.UpdatedAt : s.CreatedAt
                }).ToList();

                return new ShiftReportResponse
                {
                    Shifts = shiftReports,
                    TotalShifts = shifts.Count,
                    ActiveShifts = shifts.Count(s => s.IsActive),
                    StrictModeShifts = shifts.Count(s => s.Mode == ShiftMode.Strict),
                    OpenModeShifts = shifts.Count(s => s.Mode == ShiftMode.Open)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shift report");
                throw;
            }
        }

        public async Task<UserReportResponse> GenerateUserReportAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserShifts)
                    .ThenInclude(us => us.Shift)
                    .AsNoTracking()
                    .ToListAsync();

                var userReports = users.Select(u => new UserReportDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = !u.IsDeleted,
                    AssignedShifts = u.UserShifts?.Select(us => new UserShiftInfoDto
                    {
                        ShiftId = us.ShiftId,
                        ShiftName = us.Shift?.Name ?? "Unknown",
                        ShiftMode = us.Shift?.Mode.ToString() ?? "Unknown",
                        AssignedAt = us.AssignedAt,
                        IsActive = us.Shift?.IsActive ?? false
                    }).ToList()
                }).ToList();

                return new UserReportResponse
                {
                    Users = userReports,
                    TotalUsers = users.Count,
                    ActiveUsers = users.Count(u => !u.IsDeleted),
                    UsersWithShifts = users.Count(u => u.UserShifts?.Any() == true)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user report");
                throw;
            }
        }

        public async Task<ShiftReportDto> GetShiftDetailsReportAsync(string shiftId)
        {
            try
            {
                var shift = await _context.Shifts
                    .Include(s => s.UserShifts)
                    .ThenInclude(us => us.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == shiftId);

                if (shift == null)
                {
                    _logger.LogWarning("Shift with ID {ShiftId} not found", shiftId);
                    return null;
                }

                return new ShiftReportDto
                {
                    Id = shift.Id,
                    Name = shift.Name,
                    Description = shift.Description,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    Mode = shift.Mode.ToString(),
                    IsActive = shift.IsActive,
                    AssignedUsersCount = shift.UserShifts?.Count ?? 0,
                    LastModified = shift.UpdatedAt > shift.CreatedAt ? shift.UpdatedAt : shift.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shift details report for shift {ShiftId}", shiftId);
                throw;
            }
        }

        public async Task<UserReportDto> GetUserDetailsReportAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserShifts)
                    .ThenInclude(us => us.Shift)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    return null;
                }

                return new UserReportDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = !user.IsDeleted,
                    AssignedShifts = user.UserShifts?.Select(us => new UserShiftInfoDto
                    {
                        ShiftId = us.ShiftId,
                        ShiftName = us.Shift?.Name ?? "Unknown",
                        ShiftMode = us.Shift?.Mode.ToString() ?? "Unknown",
                        AssignedAt = us.AssignedAt,
                        IsActive = us.Shift?.IsActive ?? false
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user details report for user {UserId}", userId);
                throw;
            }
        }
    }
}
