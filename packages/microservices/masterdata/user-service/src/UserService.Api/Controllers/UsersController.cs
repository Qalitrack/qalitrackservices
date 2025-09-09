
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.User;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Roles;
using UserService.Core.Interfaces.Services;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class UsersController(
        IUserService userService,
        IUserRoleService userRoleService,
        IMapper mapper,
        ILogger<UsersController> logger)
        : ControllerBase
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UsersController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUserRoleService _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));


        [HttpGet("{id}")]
        [Authorize(Policy = "users.view")]
        public async Task<UserReadDto> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }
            return user;
        }

        [HttpPost]
        [Authorize(Policy = "users.create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateAsync(createUserDto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating user");
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
                    "23505" => pgEx.ConstraintName switch
                    {
                        "IX_Users_Email" => "A user with this email already exists. Please use a different email address.",
                        "IX_Users_Username" => "This username is already taken. Please choose a different username.",
                        _ => $"Duplicate entry detected: {pgEx.ConstraintName}"
                    },
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error creating user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while creating the user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "users.manage")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                UserReadDto result = await _userService.UpdateAsync(id, updateUserDto);
                if (result == null)
                {
                    return NotFound(new { 
                        Success = false, 
                        Message = $"User with ID {id} not found", 
                        Errors = (string[])null, 
                        StatusCode = 404 
                    });
                }
                return Ok(result);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating user with ID: {UserId}", id);
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
                    "23505" => pgEx.ConstraintName switch
                    {
                        "IX_Users_Email" => "A user with this email already exists. Please use a different email address.",
                        "IX_Users_Username" => "This username is already taken. Please choose a different username.",
                        _ => $"Duplicate entry detected: {pgEx.ConstraintName}"
                    },
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error updating user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while updating the user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "users.manage")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { 
                        Success = false, 
                        Message = $"User with ID {id} not found", 
                        Errors = (string[])null, 
                        StatusCode = 404 
                    });
                }
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => "Cannot delete user - referenced by other records",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error deleting user: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while deleting the user", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }


        [HttpPatch("{id}/restore")]
        [Authorize(Policy = "users.manage")]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _userService.RestoreAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Deleted user with ID {id} not found" });
            }
            return Ok(new { message = "User restored successfully" });
        }

        [HttpGet("{userId}/permissions")]
        [Authorize(Policy = "users.view")]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            // Ensure the user is provided
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Ok(new { permissions = new List<string>(), sources = new { roles = new List<string>(), direct = new List<string>() } });
            }

            // Get the permissions for the user from the service layer
            var permissions = await _userService.GetUserPermissionsAsync(userId);

            // If permissions are null or empty, handle gracefully
            if (permissions == null || !permissions.Any())
            {
                return Ok(new { permissions = new List<string>(), sources = new { roles = new List<string>(), direct = new List<string>() } });
            }

            // Return the list of permission names (strings)
            var permissionNames = permissions.Select(p => p.Name).ToList();
            
            // For debugging - show permission sources
            var result = new
            {
                permissions = permissionNames,
                count = permissionNames.Count,
                sources = new
                {
                    note = "Check /api/v1/users/{userId}/permissions/debug for detailed sources"
                }
            };
            
            return Ok(result);
        }


        [HttpGet("paged")]
        [Authorize(Policy = "users.view")]
        public async Task<PagedResult<UserReadDto>> GetPaged([FromQuery] PaginationParameters parameters)
        {
            try
            {
                return await _userService.GetPagedAsync(parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged users");
                throw;
            }
        }

        [HttpGet("deleted/paged")]
        [Authorize(Policy = "users.manage")]
        public async Task<PagedResult<UserReadDto>> GetDeletedPaged([FromQuery] PaginationParameters parameters)
        {
            try
            {
                return await _userService.GetDeletedPagedAsync(parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged deleted users");
                throw;
            }
        }
        [HttpPost("{userId}/reset-password")]
        [Authorize(Policy = "users.manage")]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            try
            {
                var result = await _userService.ResetUserPasswordAsync(userId);
                if (!result)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = $"User with ID {userId} not found or password reset failed",
                        Errors = (string[])null,
                        StatusCode = 404
                    });
                }
                return Ok(new
                {
                    Success = true,
                    Message = "Password reset initiated successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for password reset: {UserId}", userId);
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = (string[])null,
                    StatusCode = 404
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for password reset: {UserId}", userId);
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = (string[])null,
                    StatusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while resetting the password",
                    Errors = (string[])null,
                    StatusCode = 500
                });
            }
        }
        
        [HttpGet("{userId}/roles")]
        [Authorize(Policy = "users.view")]
       
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _userService.GetUserRolesByUserIdAsync(userId);
                var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);
                var enumerable = roleDtos as RoleDto[] ?? roleDtos.ToArray();
                return Ok(new
                {
                    Success = true,
                    Roles = enumerable,
                    Count = enumerable.Count()
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for roles retrieval: {UserId}", userId);
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = (string[])null,
                    StatusCode = 404
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for roles retrieval: {UserId}", userId);
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = (string[])null,
                    StatusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user: {UserId}", userId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving user roles",
                    Errors = (string[])null,
                    StatusCode = 500
                });
            }
        }
    }
}