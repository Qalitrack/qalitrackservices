using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Report;
using UserService.Core.DTOs.Common;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Services;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Services
{
    public class ReportService(
        UserServiceDbContext context,
        ILogger<ReportService> logger)
        : IReportService
    {
        private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<ReportService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<PagedResult<ShiftReportDto>> GenerateShiftReportAsync(PaginationParameters parameters)
        {
            try
            {
                var query = _context.Shifts
                    .Include(s => s.UserShifts)
                    .ThenInclude(us => us.User)
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(parameters.Search))
                {
                    query = query.Where(s => s.Name.Contains(parameters.Search) || 
                                           s.Description.Contains(parameters.Search));
                }

                query = parameters.SortBy?.ToLower() switch
                {
                    "name" => parameters.SortDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                    "mode" => parameters.SortDescending ? query.OrderByDescending(s => s.Mode) : query.OrderBy(s => s.Mode),
                    "isactive" => parameters.SortDescending ? query.OrderByDescending(s => s.IsActive) : query.OrderBy(s => s.IsActive),
                    "starttime" => parameters.SortDescending ? query.OrderByDescending(s => s.StartTime) : query.OrderBy(s => s.StartTime),
                    _ => parameters.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
                };

                var totalCount = await query.CountAsync();
                
                var shifts = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var shiftReports = shifts.Select(s => new ShiftReportDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Mode = s.Mode == ShiftMode.Open ? "Open" : "Strict", // Corrected interpretation
                    IsActive = s.IsActive,
                    CreatedBy = s.CreatedBy,
                    UpdatedBy = s.UpdatedBy,
                    AssignedUsersCount = s.UserShifts?.Count ?? 0,
                    LastModified = s.UpdatedAt > s.CreatedAt ? s.UpdatedAt : s.CreatedAt,
                    AssignedUsers = s.UserShifts?.Where(us => us.User != null && !us.User.IsDeleted)
                                                .Select(us => new AssignedUserDto
                                                {
                                                    Id = us.User.Id,
                                                    Email = us.User.Email,
                                                    FirstName = us.User.FirstName,
                                                    LastName = us.User.LastName,
                                                    AssignedAt = us.AssignedAt,
                                                    IsActive = !us.User.IsDeleted,
                                                }).ToList() ?? new List<AssignedUserDto>()
                }).ToList();

                return new PagedResult<ShiftReportDto>
                {
                    Items = shiftReports,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shift report");
                throw;
            }
        }

        public async Task<PagedResult<UserReportDto>> GenerateUserReportAsync(PaginationParameters parameters)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.UserShifts)
                    .ThenInclude(us => us.Shift)
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(parameters.Search))
                {
                    query = query.Where(u => u.Email.Contains(parameters.Search) || 
                                           u.FirstName.Contains(parameters.Search) ||
                                           u.LastName.Contains(parameters.Search));
                }

                query = parameters.SortBy?.ToLower() switch
                {
                    "email" => parameters.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "firstname" => parameters.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                    "lastname" => parameters.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                    "isactive" => parameters.SortDescending ? query.OrderByDescending(u => u.IsDeleted) : query.OrderBy(u => u.IsDeleted),
                    _ => parameters.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
                };

                var totalCount = await query.CountAsync();
                
                var users = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var userReports = users.Select(u => new UserReportDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = !u.IsDeleted,
                    CreatedBy = u.CreatedBy,
                    UpdatedBy = u.UpdatedBy,
                    LastModified = u.UpdatedAt > u.CreatedAt ? u.UpdatedAt : u.CreatedAt,
                    AssignedShifts = u.UserShifts
                        .Where(us => us.Shift != null)
                        .Select(us => new UserShiftInfoDto
                        {
                            ShiftId = us.ShiftId,
                            ShiftName = us.Shift!.Name,
                            ShiftMode = us.Shift.Mode == ShiftMode.Open ? "Open" : "Strict",
                            AssignedAt = us.AssignedAt,
                            IsActive = us.Shift.IsActive
                        })
                        .ToList()
                }).ToList();

                return new PagedResult<UserReportDto>
                {
                    Items = userReports,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount
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
                    Mode = shift.Mode == ShiftMode.Open ? "Open" : "Strict", // Corrected
                    IsActive = shift.IsActive,
                    AssignedUsersCount = shift.UserShifts?.Count ?? 0,
                    LastModified = shift.UpdatedAt > shift.CreatedAt ? shift.UpdatedAt : shift.CreatedAt,
                    AssignedUsers = shift.UserShifts?.Where(us => us.User != null && !us.User.IsDeleted)
                                                  .Select(us => new AssignedUserDto
                                                  {
                                                      Id = us.User.Id,
                                                      Email = us.User.Email,
                                                      FirstName = us.User.FirstName,
                                                      LastName = us.User.LastName,
                                                      AssignedAt = us.AssignedAt,
                                                      IsActive = !us.User.IsDeleted
                                                  }).ToList() ?? new List<AssignedUserDto>()
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
                        ShiftMode = us.Shift?.Mode == ShiftMode.Open ? "Open" : "Strict", // Corrected
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