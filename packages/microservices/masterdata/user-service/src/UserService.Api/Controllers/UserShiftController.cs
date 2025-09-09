using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Shift;
using UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces.Services;

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

        [HttpGet("{userId}/shifts")]
        [Authorize(Policy = "users.view")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserShifts(string userId)
        {
            try
            {
                var userShifts = await _shiftService.GetShiftsForUserAsync(userId);
                
                return Ok(new { 
                    success = true,
                    shifts = userShifts,
                    message = $"Retrieved shifts for user {userId}" 
                });
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error retrieving shifts for user {UserId}", userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error retrieving shifts for user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex) when (ex.Message == "User does not exist or is deleted")
            {
                _logger.LogWarning(ex, "Attempted to retrieve shifts for inactive user {UserId}", userId);
                return NotFound(new { 
                    Success = false, 
                    Message = "User is not active or does not exist", 
                    Errors = (string[])null, 
                    StatusCode = 404 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shifts for user {UserId}", userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while retrieving user shifts", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
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
                
                return CreatedAtAction(nameof(GetUserShifts), new { userId }, new {
                    success = true,
                    message = $"Successfully assigned shift {shiftId} to user {userId}"
                });
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error assigning shift {ShiftId} to user {UserId}", shiftId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23505" => "User is already assigned to this shift",
                    "23503" => pgEx.ConstraintName switch
                    {
                        "FK_UserShifts_Users_UserId" => "User does not exist",
                        "FK_UserShifts_Shifts_ShiftId" => "Shift does not exist",
                        _ => "Referenced record does not exist"
                    },
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error assigning shift to user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex) when (ex.Message == "User does not exist or is deleted")
            {
                _logger.LogWarning(ex, "Attempted to assign shift {ShiftId} to inactive user {UserId}", shiftId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "User is not active or does not exist", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex) when (ex.Message == "Shift not found")
            {
                _logger.LogWarning(ex, "Attempted to assign non-existent shift {ShiftId} to user {UserId}", shiftId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "Shift does not exist", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning shift {ShiftId} to user {UserId}", shiftId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while assigning shift to user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
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
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error removing shift {ShiftId} from user {UserId}", shiftId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => pgEx.ConstraintName switch
                    {
                        "FK_UserShifts_Users_UserId" => "User does not exist",
                        "FK_UserShifts_Shifts_ShiftId" => "Shift does not exist",
                        _ => "Referenced record does not exist"
                    },
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error removing shift from user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex) when (ex.Message == "User is not assigned to this shift")
            {
                _logger.LogWarning(ex, "Attempted to remove user {UserId} from shift {ShiftId} but user is not assigned", userId, shiftId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "User is not assigned to this shift", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing shift {ShiftId} from user {UserId}", shiftId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while removing shift from user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
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
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error mass assigning shift {ShiftId} to role {RoleId}", shiftId, roleId);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23505" => "Some users are already assigned to this shift",
                    "23503" => pgEx.ConstraintName switch
                    {
                        "FK_UserShifts_Users_UserId" => "One or more users do not exist",
                        "FK_UserShifts_Shifts_ShiftId" => "Shift does not exist",
                        _ => "Referenced record does not exist"
                    },
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error mass assigning shift to role: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mass assigning shift {ShiftId} to role {RoleId}", shiftId, roleId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while mass assigning shift to role", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
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
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error mass removing shift {ShiftId} from role {RoleId}", shiftId, roleId);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => pgEx.ConstraintName switch
                    {
                        "FK_UserShifts_Users_UserId" => "One or more users do not exist",
                        "FK_UserShifts_Shifts_ShiftId" => "Shift does not exist",
                        _ => "Referenced record does not exist"
                    },
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error mass removing shift from role: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mass removing shift {ShiftId} from role {RoleId}", shiftId, roleId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while mass removing shift from role", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
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
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error getting users assigned to shift {ShiftId}", shiftId);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error getting users assigned to shift: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users assigned to shift {ShiftId}", shiftId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while getting users assigned to shift", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }
    }
}
