using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UserModule.Data;
using UserModule.Models;
using UserModule.Authorization;
using UserModule.Dtos.Permissions;

namespace UserModule.Controllers;

[Route("api/permissions")]
[ApiController]
[Authorize(AuthenticationSchemes = "Sanctum")]
public class PermissionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PermissionController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [RequirePermission("permissions.read")]
    public async Task<ActionResult<IEnumerable<PermissionReadDto>>> GetPermissions()
    {
        var permissions = await _context.Permissions.ToListAsync();
        return Ok(_mapper.Map<List<PermissionReadDto>>(permissions));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("permissions.read")]
    public async Task<ActionResult<PermissionReadDto>> GetPermission(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            return NotFound();

        return Ok(_mapper.Map<PermissionReadDto>(permission));
    }

    [HttpPost]
    [RequirePermission("permissions.create")]
    public async Task<ActionResult<PermissionReadDto>> CreatePermission(PermissionCreateDto permissionDto)
    {
        var permission = _mapper.Map<Permission>(permissionDto);
        
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, _mapper.Map<PermissionReadDto>(permission));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("permissions.update")]
    public async Task<IActionResult> UpdatePermission(Guid id, PermissionUpdateDto permissionDto)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            return NotFound();

        _mapper.Map(permissionDto, permission);
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<PermissionReadDto>(permission));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("permissions.delete")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            return NotFound();

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}