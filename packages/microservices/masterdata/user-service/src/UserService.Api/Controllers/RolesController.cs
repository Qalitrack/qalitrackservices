using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Roles;
using UserService.Core.Interfaces.Services;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.RolePermission;
using UserService.Core.Entities;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
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
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating role");
                return BadRequest(new
                {
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
                        "IX_Roles_Name" => "A role with this name already exists. Please choose a different name.",
                        _ => $"Duplicate entry detected: {pgEx.ConstraintName}"
                    },
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error creating role: {ErrorMessage}", errorMessage);
                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = (string[])null,
                    StatusCode = 400
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role");
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred while creating the role",
                    Errors = (string[])null,
                    StatusCode = 400
                });
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
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating role with ID: {RoleId}", id);
                return BadRequest(new
                {
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
                        "IX_Roles_Name" => "A role with this name already exists. Please choose a different name.",
                        _ => $"Duplicate entry detected: {pgEx.ConstraintName}"
                    },
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error updating role: {ErrorMessage}", errorMessage);
                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = (string[])null,
                    StatusCode = 400
                });
            }
            catch (KeyNotFoundException ex)
            {
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
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role with ID: {RoleId}", id);
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred while updating the role",
                    Errors = (string[])null,
                    StatusCode = 400
                });
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
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => "Cannot delete role due to existing dependencies.",
                    "23514" => "Role data failed validation.",
                    _ => "A database error occurred while deleting the role."
                };

                _logger.LogWarning(ex, "Database constraint error deleting role: {ErrorMessage}", errorMessage);
                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = (string[])null,
                    StatusCode = 400
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role with ID: {RoleId}", id);
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred while deleting the role",
                    Errors = (string[])null,
                    StatusCode = 400
                });
            }
        }



        [HttpPatch("{id}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Restore(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest("Role ID is required");

                var restored = await _roleService.RestoreAsync(id);
                if (!restored)
                    return NotFound(new { message = $"Deleted role with ID {id} not found" });

                _logger.LogInformation("Successfully restored role with ID {RoleId}", id);
                return Ok(new { message = "Role restored successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring role with ID: {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while restoring the role" });
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
                return Ok(new { Message = "Permission assigned to role successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);
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


                return Ok(new { Message = "Permission removed from role" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
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
                _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
                return StatusCode(500, "Error removing permission from role");
            }
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResult<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDeletedRoles(
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

                var result = await _roleService.GetDeletedPagedAsync(parameters);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deleted roles");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Success = false, Message = "An error occurred while retrieving deleted roles" });
            }
        }
        
        [HttpGet("{roleId}/permissions")]
        [Authorize(Policy = "roles.view")]
        [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetRolePermissions(string roleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    _logger.LogWarning("Get role permissions called with empty roleId");
                    return BadRequest("Role ID is required");
                }

                var permissions = await _roleService.GetPermissionsForRoleAsync(roleId);
                var permissionDtos = _mapper.Map<IEnumerable<PermissionDto>>(permissions);
                return Ok(permissionDtos);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Role not found when getting permissions for role {RoleId}", roleId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions for role {RoleId}", roleId);
                return StatusCode(500, "An error occurred while retrieving role permissions");
            }
        }
        
        [HttpGet("permissions/deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResult<RolePermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDeletedRolePermissions(
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

                var result = await _roleService.GetDeletedRolePermissionsPagedAsync(parameters);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deleted role permissions");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Success = false, Message = "An error occurred while retrieving deleted role permissions" });
            }
        }
    }
}