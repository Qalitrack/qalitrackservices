using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UserModule.Data;
using UserModule.Models;
using UserModule.Services;
using UserModule.Authorization;
using System.IdentityModel.Tokens.Jwt;
using UserModule.Dtos.Roles;
using UserModule.Dtos.Users;

namespace UserModule.Controllers;

[Route("api/user-management")]
[ApiController]
[Authorize(AuthenticationSchemes = "Sanctum")]
public class UserManagementController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;

    public UserManagementController(AppDbContext context, IMapper mapper, IPermissionService permissionService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
    }

    [HttpPost("assign-role")]
    [RequirePermission("users.assign-roles")]
    public async Task<IActionResult> AssignRoleToUser(AssignRoleDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { Message = "Role not found" });

        var existingAssignment = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

        if (existingAssignment != null)
            return BadRequest(new { Message = "User already has this role" });

        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var assignedBy))
            return Unauthorized(new { Message = "Invalid token" });

        var userRole = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId,
            AssignedBy = assignedBy,
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Role assigned successfully" });
    }

    [HttpDelete("remove-role")]
    [RequirePermission("users.remove-roles")]
    public async Task<IActionResult> RemoveRoleFromUser(AssignRoleDto dto)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

        if (userRole == null)
            return NotFound(new { Message = "Role assignment not found" });

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Role removed successfully" });
    }

    [HttpGet("{userId:guid}/permissions")]
    [RequirePermission("users.read-permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        try
        {
            var userPermissions = await _permissionService.GetUserPermissionsDetailAsync(userId);
            return Ok(userPermissions);
        }
        catch (ArgumentException)
        {
            return NotFound(new { Message = "User not found" });
        }
    }

    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { Message = "Invalid token" });

        try
        {
            var userPermissions = await _permissionService.GetUserPermissionsDetailAsync(userId);
            return Ok(userPermissions);
        }
        catch (ArgumentException)
        {
            return NotFound(new { Message = "User not found" });
        }
    }

    [HttpPut("{userId:guid}/toggle-active")]
    [RequirePermission("users.update")]
    public async Task<IActionResult> ToggleUserActiveStatus(Guid userId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userIdFromToken))
            return Unauthorized(new { Message = "Invalid token" });

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        // Prevent users from toggling their own active status
        if (userIdFromToken == userId)
            return BadRequest(new { Message = "You cannot toggle your own active status" });

        // Toggle the IsActive status
        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userIdFromToken;

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Log the audit event
        await LogAuditEvent(userIdFromToken, "ToggleUserActiveStatus", $"User {user.Email} active status set to {user.IsActive}", user.Email);

        return Ok(new { Message = $"User active status set to {user.IsActive}" });
    }

    private async Task LogAuditEvent(Guid? userId, string action, string description, string email)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}