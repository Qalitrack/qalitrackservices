using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Permissions;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : BaseController
    {
        private readonly IPermissionsService _permissionService;
        private readonly IMapper _mapper;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionsService permissionService, IMapper mapper, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "permissions.view")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var permissions = await _permissionService.GetAllAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                return StatusCode(500, "An error occurred while retrieving permissions");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "permissions.view")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var permission = await _permissionService.GetByIdAsync(id);
                if (permission == null)
                    return NotFound();

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting permission with ID: {id}");
                return StatusCode(500, "An error occurred while retrieving the permission");
            }
        }

        [HttpPost]
        [Authorize(Policy = "permissions.manage")]
        public async Task<IActionResult> Create([FromBody] CreatePermissionDto createPermissionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var permissionDto = await _permissionService.CreateAsync(createPermissionDto);
                return CreatedAtAction(nameof(GetById), new { id = permissionDto.Id }, permissionDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, "An error occurred while creating the permission");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "permissions.manage")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var permissionDto = await _permissionService.UpdateAsync(id, updatePermissionDto);
                if (permissionDto == null)
                    return NotFound();

                return Ok(permissionDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permission with ID: {id}");
                return StatusCode(500, "An error occurred while updating the permission");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "permissions.manage")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _permissionService.DeleteAsync(id);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Permission deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting permission with ID: {id}");
                return StatusCode(500, "An error occurred while deleting the permission");
            }
        }

        [HttpGet("{permissionId}/roles")]
        [Authorize(Policy = "permissions.view")]
        public async Task<IActionResult> GetRolesForPermission(string permissionId)
        {
            try
            {
                var roles = await _permissionService.GetRolesForPermissionAsync(permissionId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting roles for permission ID: {permissionId}");
                return StatusCode(500, "An error occurred while retrieving roles for the permission");
            }
        }
    }
}