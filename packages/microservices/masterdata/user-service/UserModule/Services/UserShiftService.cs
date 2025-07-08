using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserModule.Data;
using UserModule.Dtos.Shift;
using UserModule.Models;
using UserModule.Services.Interfaces;

namespace UserModule.Services
{
    public class UserShiftService : IUserShiftService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserShiftService> _logger;
        private readonly IShiftService _shiftService;

        public UserShiftService(
            AppDbContext dbContext,
            IMapper mapper,
            ILogger<UserShiftService> logger,
            IShiftService shiftService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        }

        public async Task<IEnumerable<UserShiftReadDto>> GetAllUserShiftsAsync()
        {
            var userShifts = await _dbContext.UserShifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserShiftReadDto>>(userShifts);
        }

        public async Task<IEnumerable<UserShiftReadDto>> GetUserShiftsAsync(Guid userId)
        {
            var userShifts = await _dbContext.UserShifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserShiftReadDto>>(userShifts);
        }

        public async Task<IEnumerable<UserShiftReadDto>> GetUsersInShiftAsync(Guid shiftId)
        {
            var userShifts = await _dbContext.UserShifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .Where(us => us.ShiftId == shiftId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserShiftReadDto>>(userShifts);
        }

        public async Task<UserShiftReadDto> AssignShiftToUserAsync(UserShiftAssignDto assignDto)
        {
            // Check if user exists
            var user = await _dbContext.Users.FindAsync(assignDto.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {assignDto.UserId} not found");
            }

            // Check if shift exists
            var shift = await _dbContext.Shifts.FindAsync(assignDto.ShiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Shift with ID {assignDto.ShiftId} not found");
            }

            // Check if user is already assigned to this shift on the same date
            var existingAssignment = await _dbContext.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == assignDto.UserId && 
                                         us.ShiftId == assignDto.ShiftId && 
                                         us.AssignedDate.Date == assignDto.AssignedDate.Date);

            if (existingAssignment != null)
            {
                throw new InvalidOperationException("User is already assigned to this shift on the specified date");
            }

            var userShift = new UserShift
            {
                Id = Guid.NewGuid(),
                UserId = assignDto.UserId,
                ShiftId = assignDto.ShiftId,
                AssignedDate = assignDto.AssignedDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.UserShifts.Add(userShift);
            await _dbContext.SaveChangesAsync();

            // Reload with related data for the DTO
            var createdUserShift = await _dbContext.UserShifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .FirstOrDefaultAsync(us => us.Id == userShift.Id);

            return _mapper.Map<UserShiftReadDto>(createdUserShift);
        }

        public async Task<bool> UpdateUserShiftAsync(Guid assignmentId, UserShiftAssignDto updateDto)
        {
            var userShift = await _dbContext.UserShifts.FindAsync(assignmentId);
            if (userShift == null)
            {
                return false;
            }

            // Check if the new shift exists
            if (await _dbContext.Shifts.FindAsync(updateDto.ShiftId) == null)
            {
                throw new KeyNotFoundException($"Shift with ID {updateDto.ShiftId} not found");
            }

            // Check for conflicts
            var hasConflict = await _dbContext.UserShifts
                .AnyAsync(us => us.Id != assignmentId &&
                              us.UserId == updateDto.UserId &&
                              us.ShiftId == updateDto.ShiftId &&
                              us.AssignedDate.Date == updateDto.AssignedDate.Date);

            if (hasConflict)
            {
                throw new InvalidOperationException("User is already assigned to this shift on the specified date");
            }

            userShift.ShiftId = updateDto.ShiftId;
            userShift.AssignedDate = updateDto.AssignedDate;
            userShift.UpdatedAt = DateTime.UtcNow;

            _dbContext.UserShifts.Update(userShift);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserShiftAsync(Guid assignmentId)
        {
            var userShift = await _dbContext.UserShifts.FindAsync(assignmentId);
            if (userShift == null)
            {
                return false;
            }

            _dbContext.UserShifts.Remove(userShift);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<UserShiftReadDto?> GetCurrentUserShiftAsync(Guid userId)
        {
            var currentTime = DateTime.UtcNow;
            
            var userShift = await _dbContext.UserShifts
                .Include(us => us.Shift)
                .Where(us => us.UserId == userId && 
                           us.IsActive &&
                           us.AssignedDate.Date == currentTime.Date &&
                           us.Shift.IsActive)
                .OrderByDescending(us => us.CreatedAt)
                .FirstOrDefaultAsync();

            if (userShift == null)
                return null;

            return _mapper.Map<UserShiftReadDto>(userShift);
        }
    }
}
