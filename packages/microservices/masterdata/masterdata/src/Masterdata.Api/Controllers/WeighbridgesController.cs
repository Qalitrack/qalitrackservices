using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Weighbridge;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masterdata.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class WeighbridgesController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<WeighbridgesController> _logger;

    public WeighbridgesController(
        IWeighbridgeService weighbridgeService,
        ILogger<WeighbridgesController> logger)
    {
        _weighbridgeService = weighbridgeService ?? throw new ArgumentNullException(nameof(weighbridgeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all weighbridges with pagination and search
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WeighbridgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<WeighbridgeDto>>> GetWeighbridges(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _weighbridgeService.GetPagedWeighbridgesAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weighbridges");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving weighbridges");
        }
    }

    /// <summary>
    /// Get a specific weighbridge by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WeighbridgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeighbridgeDto>> GetWeighbridge(Guid id)
    {
        try
        {
            var weighbridge = await _weighbridgeService.GetByIdAsync(id);
            if (weighbridge == null)
            {
                return NotFound($"Weighbridge with ID {id} not found.");
            }
            return Ok(weighbridge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving weighbridge with ID: {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the weighbridge");
        }
    }

    /// <summary>
    /// Create a new weighbridge
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WeighbridgeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeighbridgeDto>> CreateWeighbridge([FromBody] CreateWeighbridgeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _weighbridgeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetWeighbridge), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weighbridge");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the weighbridge");
        }
    }

    /// <summary>
    /// Update an existing weighbridge
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WeighbridgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateWeighbridge(Guid id, [FromBody] UpdateWeighbridgeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _weighbridgeService.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound($"Weighbridge with ID {id} not found.");
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating weighbridge with ID: {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the weighbridge");
        }
    }

    /// <summary>
    /// Delete a weighbridge
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteWeighbridge(Guid id)
    {
        try
        {
            var success = await _weighbridgeService.DeleteAsync(id);
            if (!success)
            {
                return NotFound($"Weighbridge with ID {id} not found.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting weighbridge with ID: {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the weighbridge");
        }
    }

    /// <summary>
    /// Check if a location is available for a weighbridge
    /// </summary>
    [HttpGet("check-location-availability")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> CheckLocationAvailability(
        [FromQuery] string location,
        [FromQuery] Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest("Location is required");
            }

            var isAvailable = await _weighbridgeService.IsLocationAvailableAsync(location, excludeId);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking location availability for: {location}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking location availability");
        }
    }
}
