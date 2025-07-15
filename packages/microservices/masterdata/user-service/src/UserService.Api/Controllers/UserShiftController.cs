using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Shift;
using UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        private readonly ILogger<UserShiftController> _logger;

        public UserShiftController(
            IShiftService shiftService,
            ILogger<UserShiftController> logger)
        {
            _shiftService = shiftService;
            _logger = logger;
        }

        [HttpGet("{userId}/shift/{shiftId}")]
        [Authorize(Policy = "users.view")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserShift(string userId, string shiftId)
        {
            try
            {
                bool isAssigned = (bool)await _shiftService.IsUserAssignedToShiftAsync(userId, shiftId);
                var message = isAssigned 
                    ? $"User {userId} is assigned to shift {shiftId}"
                    : $"User {userId} is not assigned to shift {shiftId}";
                
                return Ok(new { 
                    isAssigned, 
                    message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is assigned to shift {ShiftId}", userId, shiftId);
                throw;
            }
        }

        [HttpPost("{userId}/shift/{shiftId}")]
        [Authorize(Policy = "users.manage")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AssignShiftToUser(string userId, string shiftId)
        {
            try
            {
                var result = await _shiftService.AssignUserToShiftAsync(userId, shiftId);
                if (!result)
                {
                    return BadRequest(new { 
                        success = false,
                        message = $"Failed to assign shift {shiftId} to user {userId}"
                    });
                }
                
                return CreatedAtAction(nameof(GetUserShift), new { userId, shiftId }, new {
                    success = true,
                    message = $"Successfully assigned shift {shiftId} to user {userId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning shift {ShiftId} to user {UserId}", shiftId, userId);
                throw;
            }
        }

        [HttpDelete("{userId}/shift/{shiftId}")]
        [Authorize(Policy = "users.manage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveShiftFromUser(string userId, string shiftId)
        {
            try
            {
                var result = await _shiftService.RemoveUserFromShiftAsync(userId, shiftId);
                if (!result)
                {
                    return BadRequest(new { 
                        success = false,
                        message = $"Failed to remove shift {shiftId} from user {userId}"
                    });
                }
                
                return Ok(new {
                    success = true,
                    message = $"Successfully removed shift {shiftId} from user {userId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing shift {ShiftId} from user {UserId}", shiftId, userId);
                throw;
            }
        }

        [HttpPost("role/{roleId}/shift/{shiftId}")]
        [Authorize(Policy = "users.manage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MassAssignShiftToRole(string roleId, string shiftId)
        {
            try
            {
                var result = await _shiftService.MassAssignShiftToRoleAsync(roleId, shiftId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mass assigning shift {ShiftId} to role {RoleId}", shiftId, roleId);
                throw;
            }
        }

        [HttpDelete("role/{roleId}/shift/{shiftId}")]
        [Authorize(Policy = "users.manage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MassRemoveUsersFromShiftByRole(string roleId, string shiftId)
        {
            try
            {
                var result = await _shiftService.MassRemoveUsersFromShiftByRoleAsync(roleId, shiftId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mass removing shift {ShiftId} from role {RoleId}", shiftId, roleId);
                throw;
            }
        }

        [HttpGet("shift/{shiftId}/users")]
        [Authorize(Policy = "users.view")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUsersAssignedToShift(string shiftId)
        {
            try
            {
                var result = await _shiftService.GetUsersAssignedToShiftAsync(shiftId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users assigned to shift {ShiftId}", shiftId);
                throw;
            }
        }
    }
} 