using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UserModule.Data;
using UserModule.Models;
using UserModule.Authorization;
using UserModule.Dtos.Roles;

namespace UserModule.Controllers;

[Route("api/roles")]
[ApiController]
[Authorize(AuthenticationSchemes = "Sanctum")]
public class RoleController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public RoleController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [RequirePermission("roles.read")]
    public async Task<ActionResult<IEnumerable<RoleReadDto>>> GetRoles()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();

        return Ok(_mapper.Map<List<RoleReadDto>>(roles));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("roles.read")]
    public async Task<ActionResult<RoleReadDto>> GetRole(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound();

        return Ok(_mapper.Map<RoleReadDto>(role));
    }

    [HttpPost]
    [RequirePermission("roles.create")]
    public async Task<ActionResult<RoleReadDto>> CreateRole(RoleCreateDto roleDto)
    {
        var role = _mapper.Map<Role>(roleDto);
        
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, _mapper.Map<RoleReadDto>(role));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("roles.update")]
    public async Task<IActionResult> UpdateRole(Guid id, RoleUpdateDto roleDto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return NotFound();

        _mapper.Map(roleDto, role);
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<RoleReadDto>(role));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("roles.delete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return NotFound();

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{roleId:guid}/permissions")]
    [RequirePermission("roles.assign-permissions")]
    public async Task<IActionResult> AssignPermissionsToRole(Guid roleId, AssignPermissionToRoleDto dto)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return NotFound("Role not found");

        // Remove existing permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new permissions
        foreach (var permissionId in dto.PermissionIds)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission != null)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Permissions assigned successfully" });
    }
}