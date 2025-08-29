using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.Interfaces.Services;
using UserService.Core.DTOs.Common;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController(
        IUserRoleService userRoleService,
        ILogger<UserRoleController> logger)
        : ControllerBase
    {
        private readonly IUserRoleService _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        private readonly ILogger<UserRoleController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpPost("{userId}/roles/{roleId}")]
        [Authorize(Policy = "users.manage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRoleToUser(string userId, string roleId)
        {
            try
            {
                var result = await _userRoleService.AddUserToRoleAsync(userId, roleId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error assigning role {RoleId} to user {UserId}", roleId, userId);
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
                    "23505" => "User is already assigned to this role",
                    "23503" => pgEx.ConstraintName switch
                    {
                        "FK_UserRoles_Users_UserId" => "User does not exist",
                        "FK_UserRoles_Roles_RoleId" => "Role does not exist",
                        _ => "Referenced record does not exist"
                    },
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error assigning role to user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while assigning role to user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        [Authorize(Policy = "users.manage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleId)
        {
            try
            {
                var result = await _userRoleService.RemoveUserFromRoleAsync(userId, roleId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error removing role {RoleId} from user {UserId}", roleId, userId);
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
                        "FK_UserRoles_Users_UserId" => "User does not exist",
                        "FK_UserRoles_Roles_RoleId" => "Role does not exist",
                        _ => "Referenced record does not exist"
                    },
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error removing role from user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while removing role from user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDeletedUserRoles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                var parameters = new PaginationParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _userRoleService.GetDeletedPagedAsync(parameters);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deleted user-role relationships");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Success = false, Message = "An error occurred while retrieving deleted user-role relationships" });
            }
        }
    }
}