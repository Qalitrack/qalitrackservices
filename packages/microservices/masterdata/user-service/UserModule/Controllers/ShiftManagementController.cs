using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UserModule.Data;
using UserModule.Models;
using UserModule.Services;
using UserModule.Authorization;
using System.IdentityModel.Tokens.Jwt;
using UserModule.Dtos.Shift;
using UserModule.Services.Interfaces;

namespace UserModule.Controllers;

[Route("api/shift-management")]
[ApiController]
[Authorize(AuthenticationSchemes = "Sanctum")]
public class ShiftManagementController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly IUserShiftService _userShiftService;
    private readonly IMapper _mapper;
    private readonly ILogger<ShiftManagementController> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public ShiftManagementController(
        IShiftService shiftService,
        IUserShiftService userShiftService,
        IMapper mapper,
        ILogger<ShiftManagementController> logger,
        AppDbContext dbContext,
        IPermissionService permissionService)
    {
        _shiftService = shiftService;
        _userShiftService = userShiftService;
        _mapper = mapper;
        _logger = logger;
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    #region Shift CRUD Operations

    [HttpGet("shifts")]
    [RequirePermission("shifts.read")]
    public async Task<ActionResult<IEnumerable<ShiftReadDto>>> GetAllShifts()
    {
        try
        {
            var shifts = await _shiftService.GetAllShiftsAsync();
            return Ok(shifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all shifts");
            return StatusCode(500, new { Message = "Internal server error while retrieving shifts" });
        }
    }

    [HttpGet("shifts/{id}")]
    [RequirePermission("shifts.read")]
    public async Task<ActionResult<ShiftReadDto>> GetShiftById(Guid id)
    {
        try
        {
            var shift = await _shiftService.GetShiftByIdAsync(id);
            if (shift == null)
            {
                return NotFound(new { Message = $"Shift with ID {id} not found" });
            }
            return Ok(shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while retrieving shift with ID {id}");
            return StatusCode(500, new { Message = "Internal server error while retrieving shift" });
        }
    }

    [HttpPost("shifts")]
    [RequirePermission("shifts.create")]
    public async Task<ActionResult<ShiftReadDto>> CreateShift([FromBody] ShiftCreateDto shiftCreateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdShift = await _shiftService.CreateShiftAsync(shiftCreateDto);
            return CreatedAtAction(nameof(GetShiftById), new { id = createdShift.Id }, createdShift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating shift");
            return StatusCode(500, new { Message = "Internal server error while creating shift" });
        }
    }

    [HttpPut("shifts/{id}")]
    [RequirePermission("shifts.update")]
    public async Task<IActionResult> UpdateShift(Guid id, [FromBody] ShiftCreateDto shiftUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _shiftService.UpdateShiftAsync(id, shiftUpdateDto);
            if (!result)
            {
                return NotFound(new { Message = $"Shift with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while updating shift with ID {id}");
            return StatusCode(500, new { Message = "Internal server error while updating shift" });
        }
    }

    [HttpDelete("shifts/{id}")]
    [RequirePermission("shifts.delete")]
    public async Task<IActionResult> DeleteShift(Guid id)
    {
        try
        {
            var result = await _shiftService.DeleteShiftAsync(id);
            if (!result)
            {
                return NotFound(new { Message = $"Shift with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while deleting shift with ID {id}");
            return StatusCode(500, new { Message = "Internal server error while deleting shift" });
        }
    }

    #endregion

    #region Shift Mode Operations

    [HttpPut("shifts/{shiftId}/mode/{mode}")]
    [RequirePermission("shifts.mode.update")]
    public async Task<IActionResult> UpdateShiftMode(Guid shiftId, string mode)
    {
        if (string.IsNullOrWhiteSpace(mode) || (mode != "Strict" && mode != "NonStrict"))
        {
            return BadRequest(new { Message = "Invalid mode. Must be either 'Strict' or 'NonStrict'." });
        }

        try
        {
            var result = await _shiftService.UpdateShiftModeAsync(shiftId, mode);
            if (!result)
            {
                return NotFound(new { Message = $"Shift with ID {shiftId} not found" });
            }
            return Ok(new { Message = $"Shift mode updated to {mode} successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating shift mode for shift {shiftId}");
            return StatusCode(500, new { Message = "Internal server error while updating shift mode" });
        }
    }

    #endregion

    #region User Shift Assignment Operations

    [HttpGet("user-shifts")]
    [RequirePermission("shifts.read-user-shifts")]
    public async Task<ActionResult<IEnumerable<UserShiftReadDto>>> GetAllUserShifts()
    {
        try
        {
            var userShifts = await _userShiftService.GetAllUserShiftsAsync();
            return Ok(userShifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user shifts");
            return StatusCode(500, new { Message = "Internal server error while retrieving user shifts" });
        }
    }

    [HttpGet("users/{userId}/shifts")]
    [RequirePermission("shifts.read-user-shifts")]
    public async Task<ActionResult<IEnumerable<UserShiftReadDto>>> GetUserShifts(Guid userId)
    {
        try
        {
            var userShifts = await _userShiftService.GetUserShiftsAsync(userId);
            return Ok(userShifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while retrieving shifts for user {userId}");
            return StatusCode(500, new { Message = "Internal server error while retrieving user shifts" });
        }
    }

    [HttpGet("my-shifts")]
    public async Task<ActionResult<IEnumerable<UserShiftReadDto>>> GetMyShifts()
    {
        try
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            var userShifts = await _userShiftService.GetUserShiftsAsync(userId);
            return Ok(userShifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving current user's shifts");
            return StatusCode(500, new { Message = "Internal server error while retrieving your shifts" });
        }
    }

    [HttpGet("my-current-shift")]
    public async Task<ActionResult<UserShiftReadDto>> GetMyCurrentShift()
    {
        try
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            var currentShift = await _userShiftService.GetCurrentUserShiftAsync(userId);
            if (currentShift == null)
            {
                return NotFound(new { Message = "You don't have any active shift assignments for today." });
            }

            return Ok(currentShift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving current user's shift");
            return StatusCode(500, new { Message = "Internal server error while retrieving your current shift" });
        }
    }

    [HttpGet("shifts/{shiftId}/users")]
    [RequirePermission("shifts.read-user-shifts")]
    public async Task<ActionResult<IEnumerable<UserShiftReadDto>>> GetUsersInShift(Guid shiftId)
    {
        try
        {
            var userShifts = await _userShiftService.GetUsersInShiftAsync(shiftId);
            return Ok(userShifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while retrieving users in shift {shiftId}");
            return StatusCode(500, new { Message = "Internal server error while retrieving users in shift" });
        }
    }

    [HttpPost("user-shifts/assign")]
    [RequirePermission("shifts.assign-user")]
    public async Task<ActionResult<UserShiftReadDto>> AssignShiftToUser([FromBody] UserShiftAssignDto assignDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var assignedBy))
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            // Check if the shift is in strict mode and if the user is already assigned to another shift
            var shift = await _dbContext.Shifts.FindAsync(assignDto.ShiftId);
            if (shift == null)
            {
                return NotFound(new { Message = $"Shift with ID {assignDto.ShiftId} not found" });
            }

            if (shift.Mode == ShiftMode.Strict)
            {
                var existingAssignment = await _dbContext.UserShifts
                    .Include(us => us.Shift)
                    .Where(us => us.UserId == assignDto.UserId && 
                               us.AssignedDate.Date == assignDto.AssignedDate.Date &&
                               us.Shift.Mode == ShiftMode.Strict)
                    .FirstOrDefaultAsync();

                if (existingAssignment != null && existingAssignment.ShiftId != assignDto.ShiftId)
                {
                    return BadRequest(new { Message = "User is already assigned to another strict shift on this date" });
                }
            }

            var result = await _userShiftService.AssignShiftToUserAsync(assignDto);
            return CreatedAtAction(nameof(GetUserShifts), new { userId = assignDto.UserId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning shift {assignDto.ShiftId} to user {assignDto.UserId}");
            return StatusCode(500, new { Message = "Internal server error while assigning shift to user" });
        }
    }

    [HttpPut("user-shifts/{assignmentId}")]
    [RequirePermission("shifts.update-user-shifts")]
    public async Task<IActionResult> UpdateUserShift(Guid assignmentId, [FromBody] UserShiftAssignDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _userShiftService.UpdateUserShiftAsync(assignmentId, updateDto);
            if (!result)
            {
                return NotFound(new { Message = $"Shift assignment with ID {assignmentId} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating shift assignment {assignmentId}");
            return StatusCode(500, new { Message = "Internal server error while updating user shift assignment" });
        }
    }

    [HttpGet("shifts/current")]
    [RequirePermission("shifts.read")]
    public async Task<ActionResult<IEnumerable<ShiftStatusDto>>> GetCurrentShifts()
    {
        try
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;
            var today = DateTime.UtcNow.Date;

            var activeShifts = await _dbContext.Shifts
                .Where(s => s.IsActive)
                .ToListAsync();

            var result = new List<ShiftStatusDto>();

            foreach (var shift in activeShifts)
            {
                var isInProgress = _shiftService.IsTimeInShift(currentTime, shift.StartTime, shift.EndTime);
                var usersInShift = await _dbContext.UserShifts
                    .Include(us => us.User)
                    .Where(us => us.ShiftId == shift.Id && 
                               us.AssignedDate.Date == today &&
                               us.IsActive)
                    .CountAsync();

                result.Add(new ShiftStatusDto
                {
                    ShiftId = shift.Id,
                    Name = shift.Name,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    IsActiveNow = isInProgress,
                    UserCount = usersInShift,
                    Mode = shift.Mode.ToString()
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving current shifts");
            return StatusCode(500, new { Message = "Internal server error while retrieving current shifts" });
        }
    }

    [HttpGet("shifts/{shiftId}/status")]
    [RequirePermission("shifts.read")]
    public async Task<ActionResult<ShiftStatusDto>> GetShiftStatus(Guid shiftId)
    {
        try
        {
            var shift = await _dbContext.Shifts.FindAsync(shiftId);
            if (shift == null)
            {
                return NotFound(new { Message = $"Shift with ID {shiftId} not found" });
            }

            var currentTime = DateTime.UtcNow.TimeOfDay;
            var today = DateTime.UtcNow.Date;

            var isInProgress = _shiftService.IsTimeInShift(currentTime, shift.StartTime, shift.EndTime);
            var userCount = await _dbContext.UserShifts
                .CountAsync(us => us.ShiftId == shiftId && 
                               us.AssignedDate.Date == today &&
                               us.IsActive);

            var result = new ShiftStatusDto
            {
                ShiftId = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                IsActiveNow = isInProgress,
                UserCount = userCount,
                Mode = shift.Mode.ToString(),
                TimeRemaining = CalculateTimeRemaining(currentTime, shift.EndTime)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while retrieving status for shift {shiftId}");
            return StatusCode(500, new { Message = "Internal server error while retrieving shift status" });
        }
    }

    [HttpDelete("user-shifts/{assignmentId}")]
    [RequirePermission("shifts.delete-user-shifts")]
    public async Task<IActionResult> RemoveUserShift(Guid assignmentId)
    {
        try
        {
            var result = await _userShiftService.RemoveUserShiftAsync(assignmentId);
            if (!result)
            {
                return NotFound(new { Message = $"Shift assignment with ID {assignmentId} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing shift assignment {assignmentId}");
            return StatusCode(500, new { Message = "Internal server error while removing user shift assignment" });
        }
    }

    private static TimeSpan? CalculateTimeRemaining(TimeSpan currentTime, TimeSpan endTime)
    {
        if (endTime < currentTime)
        {
            if (endTime < TimeSpan.FromHours(12))
            {
                return endTime + (TimeSpan.FromHours(24) - currentTime);
            }
            return null;
        }
        return endTime - currentTime;
    }

    #endregion
}