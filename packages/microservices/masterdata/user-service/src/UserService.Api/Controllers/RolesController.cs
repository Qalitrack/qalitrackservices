using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Role;
using UserService.Core.DTOs.Roles;
using UserService.Core.Interfaces;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolesController(
        IRoleService roleService,
        IMapper mapper,
        ILogger<RolesController> logger)
        : ControllerBase
    {
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<RolesController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet]
        [Authorize(Policy = "roles.view")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "roles.view")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleDto>> GetById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Role ID is required");
                }

                var role = await _roleService.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting role with ID: {RoleId}", id);
                return StatusCode(500, "An error occurred while retrieving the role");
            }
        }

        [HttpPost]
        [Authorize(Policy = "roles.manage")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var role = await _roleService.CreateAsync(createRoleDto);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = role.Id, version = "1.0" },
                    role);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the role");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "roles.manage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedRole = await _roleService.UpdateAsync(id, updateRoleDto);
                return Ok(updatedRole);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role with ID: {RoleId}", id);
                return StatusCode(500, "An error occurred while updating the role");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "roles.manage")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("Delete role called with empty ID");
                    return BadRequest("Role ID is required");
                }

                var deleted = await _roleService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                _logger.LogInformation("Successfully deleted role with ID {RoleId}", id);
                return Ok(new { Message = "Role deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role with ID: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the role");
            }
        }

        [HttpPost("{roleId}/permissions/{permissionId}")]
        [Authorize(Policy = "roles.manage")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPermissionToRole(string roleId, string permissionId)
        {
            try
            {
                _logger.LogInformation("Assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);
                
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(permissionId))
                {
                    _logger.LogWarning("Role ID or Permission ID is missing");
                    return BadRequest("Role ID and Permission ID are required");
                }

                await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
                _logger.LogInformation("Assigned permission {PermissionId} to role {RoleId}", permissionId, roleId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);
                return StatusCode(500, "Error assigning permission to role");
            }
        }

        [HttpDelete("{roleId}/permissions/{permissionId}")]
        [Authorize(Policy = "roles.manage")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemovePermissionFromRole(string roleId, string permissionId)
        {
            try
            {
                _logger.LogInformation("Removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
                
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(permissionId))
                {
                    _logger.LogWarning("Role ID or Permission ID is missing");
                    return BadRequest("Role ID and Permission ID are required");
                }

                var removed = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
                if (!removed)
                {
                    return NotFound("The specified permission was not assigned to this role");
                }

                _logger.LogInformation("Removed permission {PermissionId} from role {RoleId}", permissionId, roleId);
                
                return Ok(new { Message = "Permission removed from role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
                return StatusCode(500, "Error removing permission from role");
            }
        }
    }
}