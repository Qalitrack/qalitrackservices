using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Owner;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masterdata.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
[Produces("application/json")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    private readonly ILogger<OwnersController> _logger;

    public OwnersController(
        IOwnerService ownerService,
        ILogger<OwnersController> logger)
    {
        _ownerService = ownerService ?? throw new ArgumentNullException(nameof(ownerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all owners with pagination and search
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<OwnerDto>>> GetOwners(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _ownerService.GetPagedOwnersAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving owners");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving owners");
        }
    }

    /// <summary>
    /// Get a specific owner by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OwnerDto>> GetOwner(Guid id)
    {
        try
        {
            var owner = await _ownerService.GetByIdAsync(id);
            if (owner == null)
            {
                return NotFound($"Owner with ID {id} not found.");
            }
            return Ok(owner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving owner with ID: {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the owner");
        }
    }

    /// <summary>
    /// Create a new owner
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OwnerDto>> CreateOwner([FromBody] CreateOwnerDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _ownerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetOwner), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating owner");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the owner");
        }
    }

    /// <summary>
    /// Update an existing owner
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOwner(Guid id, [FromBody] UpdateOwnerDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _ownerService.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound($"Owner with ID {id} not found.");
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating owner with ID: {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the owner");
        }
    }

    /// <summary>
    /// Delete an owner
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOwner(Guid id)
    {
        try
        {
            var success = await _ownerService.DeleteAsync(id);
            if (!success)
            {
                return NotFound($"Owner with ID {id} not found.");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting owner with ID: {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the owner");
        }
    }

    /// <summary>
    /// Check if an owner name is available
    /// </summary>
    [HttpGet("check-name-availability")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> CheckNameAvailability(
        [FromQuery] string name,
        [FromQuery] Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name is required");
            }

            var isAvailable = await _ownerService.IsNameAvailableAsync(name, excludeId);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking name availability for: {name}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking name availability");
        }
    }

    /// <summary>
    /// Assign vehicles to an owner
    /// </summary>
    [HttpPost("{ownerId:guid}/vehicles/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignVehicles(Guid ownerId, [FromBody] IEnumerable<Guid> vehicleIds)
    {
        try
        {
            var success = await _ownerService.AssignVehiclesAsync(ownerId, vehicleIds);
            if (!success)
            {
                return NotFound($"Owner with ID {ownerId} not found or no valid vehicles provided.");
            }

            return Ok(new { message = "Vehicles assigned successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning vehicles to owner with ID: {ownerId}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while assigning vehicles");
        }
    }

    /// <summary>
    /// Remove vehicles from an owner
    /// </summary>
    [HttpPost("{ownerId:guid}/vehicles/remove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveVehicles(Guid ownerId, [FromBody] IEnumerable<Guid> vehicleIds)
    {
        try
        {
            var success = await _ownerService.RemoveVehiclesAsync(ownerId, vehicleIds);
            if (!success)
            {
                return NotFound($"Owner with ID {ownerId} not found or no valid vehicles provided.");
            }

            return Ok(new { message = "Vehicles removed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing vehicles from owner with ID: {ownerId}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing vehicles");
        }
    }

    /// <summary>
    /// Get all vehicles for an owner
    /// </summary>
    [HttpGet("{ownerId:guid}/vehicles")]
    [ProducesResponseType(typeof(IEnumerable<VehicleReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<VehicleReadDto>>> GetOwnerVehicles(Guid ownerId)
    {
        try
        {
            var vehicles = await _ownerService.GetOwnerVehiclesAsync(ownerId);
            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving vehicles for owner with ID: {ownerId}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving vehicles");
        }
    }
}
