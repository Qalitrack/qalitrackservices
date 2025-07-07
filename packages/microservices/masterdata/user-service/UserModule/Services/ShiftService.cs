using Microsoft.EntityFrameworkCore;
using UserModule.Data;
using UserModule.Models;
using UserModule.Dtos.Shift;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Linq;

namespace UserModule.Services
{ public class ShiftService : IShiftService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShiftService> _logger;
        private readonly IMapper _mapper;
        private bool _isStrictMode; // In-memory state for strict mode

        public ShiftService(
            AppDbContext dbContext, 
            IConfiguration configuration, 
            ILogger<ShiftService> logger,
            IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _isStrictMode = _configuration.GetValue<bool>("ShiftMode:IsStrictMode", false);
        }  

        public async Task<(bool CanLogin, string Message)> CanUserLoginAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserShifts)
                .ThenInclude(us => us.Shift)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return (false, "User not found.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("User {UserId} is inactive", userId);
                return (false, "Your account is inactive. Please contact an administrator.");
            }

            if (user.UserRoles.Any(ur => ur.Role?.Name == "Admin"))
            {
                _logger.LogInformation("User {UserId} is admin, allowing login", userId);
                return (true, "Admin login successful.");
            }

            if (!await IsStrictModeEnabledAsync())
            {
                _logger.LogInformation("Non-strict mode enabled, allowing login for user {UserId}", userId);
                return (true, "Login successful (non-strict mode).");
            }

            var isInActiveShift = await IsUserInActiveShiftAsync(userId);
            if (!isInActiveShift)
            {
                _logger.LogWarning("User {UserId} is not assigned to an active shift during strict mode", userId);
                return (false, "You can only log in during your assigned shift in strict mode.");
            }

            _logger.LogInformation("User {UserId} allowed to login during active shift", userId);
            return (true, "Login successful. You are within your assigned shift.");
        }

        public async Task<bool> IsUserInActiveShiftAsync(Guid userId)
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;

            return await _dbContext.UserShifts
                .Include(us => us.Shift)
                .Where(us => us.UserId == userId && us.Shift.IsActive)
                .AnyAsync(us => IsTimeInShift(currentTime, us.Shift.StartTime, us.Shift.EndTime));
        }

        public async Task<bool> IsStrictModeEnabledAsync()
        {
            try
            {
                // Could also store this in the database if persistence is needed
                return _isStrictMode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift mode, defaulting to non-strict mode");
                return false;
            }
        }

        public async Task SetStrictModeAsync(bool isStrictMode)
        {
            try
            {
                _isStrictMode = isStrictMode;
                _logger.LogInformation("Shift mode set to {Mode}", isStrictMode ? "Strict" : "NonStrict");
                await Task.CompletedTask; // For async signature
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shift mode");
                throw;
            }
        }

        public async Task LogoutUsersAfterShiftEndAsync()
        {
            if (!await IsStrictModeEnabledAsync())
            {
                _logger.LogInformation("Non-strict mode, skipping shift end logout");
                return;
            }

            var currentTime = DateTime.UtcNow.TimeOfDay;

            var usersToLogout = await _dbContext.Users
                .Include(u => u.UserShifts)
                .ThenInclude(us => us.Shift)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.LoginStatus && u.UserShifts.Any(us => us.Shift.IsActive))
                .ToListAsync();

            foreach (var user in usersToLogout)
            {
                if (user.UserRoles.Any(ur => ur.Role?.Name == "Admin"))
                {
                    _logger.LogInformation("User {UserId} is admin, skipping logout", user.Id);
                    continue;
                }

                var hasActiveShift = user.UserShifts.Any(us =>
                    us.Shift.IsActive &&
                    IsTimeInShift(currentTime, us.Shift.StartTime, us.Shift.EndTime));

                if (!hasActiveShift)
                {
                    user.LoginStatus = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    await LogAuditEventAsync(user.Id, "Logout", "User logged out due to shift end in strict mode");
                    _logger.LogInformation("User {UserId} logged out due to shift end", user.Id);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task LogAuditEventAsync(Guid? userId, string action, string description)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
              //  Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateShiftModeAsync(Guid shiftId, string mode)
        {
            var shift = await _dbContext.Shifts.FindAsync(shiftId);
            if (shift == null)
                return false;

            shift.Mode = mode == "Strict" ? ShiftMode.Strict : ShiftMode.NonStrict;
            shift.UpdatedAt = DateTime.UtcNow;
            
            _dbContext.Shifts.Update(shift);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> CanUserSignInAsync(Guid userId, Guid shiftId)
        {
            var shift = await _dbContext.Shifts.FindAsync(shiftId);
            if (shift == null || !shift.IsActive)
                return false;

            // In non-strict mode, any user can sign in
            if (shift.Mode == ShiftMode.NonStrict)
                return true;

            // In strict mode, check if user is assigned to this shift
            var now = DateTime.UtcNow;
            var isAssigned = await _dbContext.UserShifts
                .AnyAsync(us => us.UserId == userId && 
                              us.ShiftId == shiftId && 
                              us.IsActive &&
                              us.AssignedDate.Date == now.Date);

            if (!isAssigned)
                return false;

            // Check if current time is within shift hours
            var currentTime = now.TimeOfDay;
            return IsTimeInShift(currentTime, shift.StartTime, shift.EndTime);
        }

        public bool IsTimeInShift(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime < startTime) // Overnight shift (e.g., 22:00 to 06:00)
            {
                return currentTime >= startTime || currentTime <= endTime;
            }
            return currentTime >= startTime && currentTime <= endTime;
        }

        // Implement other interface methods
        public async Task<IEnumerable<ShiftReadDto>> GetAllShiftsAsync()
        {
            var shifts = await _dbContext.Shifts.ToListAsync();
            return _mapper.Map<IEnumerable<ShiftReadDto>>(shifts);
        }

        public async Task<ShiftReadDto?> GetShiftByIdAsync(Guid id)
        {
            var shift = await _dbContext.Shifts.FindAsync(id);
            return shift == null ? null : _mapper.Map<ShiftReadDto>(shift);
        }

        public async Task<ShiftReadDto> CreateShiftAsync(ShiftCreateDto shiftCreateDto)
        {
            var shift = _mapper.Map<Shift>(shiftCreateDto);
            shift.Id = Guid.NewGuid();
            shift.CreatedAt = DateTime.UtcNow;
            shift.UpdatedAt = DateTime.UtcNow;
            shift.IsActive = true;

            _dbContext.Shifts.Add(shift);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<ShiftReadDto>(shift);
        }

        public async Task<bool> UpdateShiftAsync(Guid id, ShiftCreateDto shiftUpdateDto)
        {
            var shift = await _dbContext.Shifts.FindAsync(id);
            if (shift == null)
                return false;

            _mapper.Map(shiftUpdateDto, shift);
            shift.UpdatedAt = DateTime.UtcNow;

            _dbContext.Shifts.Update(shift);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> DeleteShiftAsync(Guid id)
        {
            var shift = await _dbContext.Shifts.FindAsync(id);
            if (shift == null)
                return false;

            _dbContext.Shifts.Remove(shift);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
    }
}