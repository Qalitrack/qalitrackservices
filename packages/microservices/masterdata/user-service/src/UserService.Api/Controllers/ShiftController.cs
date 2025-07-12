using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftController : BaseController
{
    private readonly IShiftService _shiftService;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ShiftController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    

    public ShiftController(
        IShiftService shiftService,
        IUserService userService,
        ITokenService tokenService,
        ILogger<ShiftController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _shiftService = shiftService;
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetTokenFromHeader()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("Authorization", out StringValues authHeader) == true)
        {
            return authHeader.ToString().Replace("Bearer ", "");
        }
        return null;
    }

    private async Task<bool> HasPermissionAsync(string permissionName)
    {
        try
        {
            var token = GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No token found in request");
                return false;
            }

            var userId = await _tokenService.GetUserIdFromTokenAsync(token);
            if (userId == null || userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID from token");
                return false;
            }

            // Get all permissions for the user
            var userPermissions = (await _userService.GetUserPermissionsAsync(userId.ToString()))
                .Cast<object>()
                .ToList();

            _logger.LogInformation("User {userId} has the following permissions:", userId);
            foreach (var permission in userPermissions)
            {
                var permissionNameProp = permission.GetType().GetProperty("Name")?.GetValue(permission)?.ToString();
                var permissionIdProp = permission.GetType().GetProperty("Id")?.GetValue(permission)?.ToString();
                _logger.LogInformation("- {permissionName} (ID: {permissionId})", 
                    permissionNameProp, permissionIdProp);
            }

            return userPermissions.Any(p => 
            {
                var name = p.GetType().GetProperty("Name")?.GetValue(p)?.ToString();
                return string.Equals(name, permissionName, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {permission}", permissionName);
            return false;
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        if (!await HasPermissionAsync("shifts.view"))
            return Forbid();

        var shifts = await _shiftService.GetAllAsync();
        return Ok(shifts);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(string id)
    {
        if (!await HasPermissionAsync("shifts.view"))
            return Forbid();

        var shift = await _shiftService.GetByIdAsync(id);
        return shift == null ? NotFound() : Ok(shift);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateShiftDto dto)
    {
        if (!await HasPermissionAsync("shifts.manage"))
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var shift = await _shiftService.CreateAsync(dto);
            _logger.LogInformation("User created shift {ShiftId}", shift.Id);
            return CreatedAtAction(nameof(GetById), new { id = shift.Id }, shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            return BadRequest("An error occurred while creating the shift");
        }
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateShiftDto dto)
    {
        if (!await HasPermissionAsync("shifts.manage"))
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var shift = await _shiftService.UpdateAsync(id, dto);
            if (shift == null)
                return NotFound();

            _logger.LogInformation("Shift {ShiftId} was updated", id);
            return Ok(shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift with ID: {ShiftId}", id);
            return BadRequest("An error occurred while updating the shift");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await HasPermissionAsync("shifts.manage"))
            return Forbid();

        try
        {
            var result = await _shiftService.DeleteAsync(id);
            if (!result)
                return BadRequest("Failed to delete the shift or shift not found");

            _logger.LogInformation("Shift {ShiftId} was deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift with ID: {ShiftId}", id);
            return BadRequest("An error occurred while deleting the shift");
        }
    }
    
    [HttpPost("{shiftId}/assign/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignUserToShift(string userId, string shiftId)
    {
        if (!await HasPermissionAsync("shifts.assign"))
            return Forbid();

        try
        {
            var result = await _shiftService.AssignUserToShiftAsync(userId, shiftId);
            if (!result)
                return BadRequest("Failed to assign user to shift");

            _logger.LogInformation("User {UserId} was assigned to shift {ShiftId}", userId, shiftId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserId} to shift {ShiftId}", userId, shiftId);
            return BadRequest("An error occurred while assigning user to shift");
        }
    }

    [HttpPost("{shiftId}/remove/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveUserFromShift(string userId, string shiftId)
    {
        if (!await HasPermissionAsync("shifts.assign"))
            return Forbid();

        try
        {
            var result = await _shiftService.RemoveUserFromShiftAsync(userId, shiftId);
            if (!result)
                return BadRequest("Failed to remove user from shift");

            _logger.LogInformation("User {UserId} was removed from shift {ShiftId}", userId, shiftId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from shift {ShiftId}", userId, shiftId);
            return BadRequest("An error occurred while removing user from shift");
        }
    }
}