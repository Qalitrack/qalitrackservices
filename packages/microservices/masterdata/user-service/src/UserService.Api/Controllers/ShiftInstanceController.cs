using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class ShiftInstanceController : ControllerBase
{
    private readonly IShiftInstanceService _shiftInstanceService;
    private readonly ILogger<ShiftInstanceController> _logger;

    public ShiftInstanceController(
        IShiftInstanceService shiftInstanceService,
        ILogger<ShiftInstanceController> logger)
    {
        _shiftInstanceService = shiftInstanceService ?? throw new ArgumentNullException(nameof(shiftInstanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all shift instances
    /// </summary>
    /// <returns>List of all shift instances</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftInstanceResponse>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Admin {UserId} requested all shift instances", User.Identity?.Name);
            var instances = await _shiftInstanceService.GetAllAsync();
            return Ok(instances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all shift instances");
            return StatusCode(500, "An error occurred while retrieving shift instances");
        }
    }

    /// <summary>
    /// Get a specific shift instance by ID
    /// </summary>
    /// <param name="id">Shift instance ID</param>
    /// <returns>Shift instance details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftInstanceResponse>> GetByIdAsync([FromRoute] string id)
    {
        try
        {
            var instance = await _shiftInstanceService.GetByIdAsync(id);
            if (instance == null)
            {
                return NotFound($"Shift instance with ID {id} not found");
            }

            return Ok(instance);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid shift instance ID provided: {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift instance {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the shift instance");
        }
    }
    

   

    /// <summary>
    /// Delete a shift instance
    /// </summary>
    /// <param name="id">Shift instance ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] string id)
    {
        try
        {
            var success = await _shiftInstanceService.DeleteAsync(id);
            if (!success)
            {
                return NotFound($"Shift instance with ID {id} not found");
            }

            _logger.LogInformation("Admin {UserId} deleted shift instance {Id}", User.Identity?.Name, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid shift instance ID provided for deletion: {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift instance {Id}", id);
            return StatusCode(500, "An error occurred while deleting the shift instance");
        }
    }

    /// <summary>
    /// Generate multiple shift instances based on recurrence rules
    /// </summary>
    /// <param name="request">Instance generation request</param>
    /// <returns>List of generated shift instances</returns>
    [HttpPost("generate")]
    public async Task<ActionResult<IEnumerable<ShiftInstanceResponse>>> GenerateInstancesAsync(
        [FromBody] ShiftInstanceGenerateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var instances = await _shiftInstanceService.GenerateInstancesAsync(request);
            _logger.LogInformation("Admin {UserId} generated {Count} shift instances for shift {ShiftId}", 
                User.Identity?.Name, instances.Count(), request.ShiftId);
            return Ok(instances);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data provided for shift instance generation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating shift instances");
            return StatusCode(500, "An error occurred while generating shift instances");
        }
    }

    /// <summary>
    /// Get all shift instances for a specific shift
    /// </summary>
    /// <param name="shiftId">Shift ID</param>
    /// <returns>List of shift instances for the specified shift</returns>
    [HttpGet("by-shift/{shiftId}")]
    public async Task<ActionResult<IEnumerable<ShiftInstanceResponse>>> GetInstancesByShiftAsync(
        [FromRoute] string shiftId)
    {
        try
        {
            var instances = await _shiftInstanceService.GetInstancesByShiftAsync(shiftId);
            return Ok(instances);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid shift ID provided: {ShiftId}", shiftId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift instances for shift {ShiftId}", shiftId);
            return StatusCode(500, "An error occurred while retrieving shift instances");
        }
    }

    /// <summary>
    /// Get shift instances within a date range
    /// </summary>
    /// <param name="startDate">Start date (yyyy-MM-dd)</param>
    /// <param name="endDate">End date (yyyy-MM-dd)</param>
    /// <returns>List of shift instances within the date range</returns>
    [HttpGet("by-date-range")]
    public async Task<ActionResult<IEnumerable<ShiftInstanceResponse>>> GetInstancesByDateRangeAsync(
        [FromQuery, Required] DateTime startDate,
        [FromQuery, Required] DateTime endDate)
    {
        try
        {
            var instances = await _shiftInstanceService.GetInstancesByDateRangeAsync(startDate, endDate);
            return Ok(instances);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid date range provided: {StartDate} - {EndDate}", startDate, endDate);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift instances for date range {StartDate} - {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving shift instances");
        }
    }

    /// <summary>
    /// Get upcoming shift instances
    /// </summary>
    /// <param name="days">Number of days to look ahead (default: 7)</param>
    /// <returns>List of upcoming shift instances</returns>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<ShiftInstanceResponse>>> GetUpcomingInstancesAsync(
        [FromQuery] int days = 7)
    {
        try
        {
            var instances = await _shiftInstanceService.GetUpcomingInstancesAsync(days);
            return Ok(instances);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid days parameter provided: {Days}", days);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming shift instances");
            return StatusCode(500, "An error occurred while retrieving upcoming shift instances");
        }
    }

   

    /// <summary>
    /// Remove scheduled instances that fall on exception dates for a shift
    /// </summary>
    /// <param name="shiftId">Shift ID</param>
    /// <returns>Number of instances cleaned up</returns>
    [HttpPost("{shiftId}/cleanup-exceptions")]
    public async Task<ActionResult<int>> CleanupInstancesOnExceptionDatesAsync([FromRoute] string shiftId)
    {
        try
        {
            var count = await _shiftInstanceService.CleanupInstancesOnExceptionDatesAsync(shiftId);
            _logger.LogInformation("Admin {UserId} cleaned up {Count} instances on exception dates for shift {ShiftId}",
                User.Identity?.Name, count, shiftId);
            return Ok(new { cleaned = count });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid shift ID: {ShiftId}", shiftId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up exception date instances for shift {ShiftId}", shiftId);
            return StatusCode(500, "An error occurred while cleaning up instances");
        }
    }

    /// <summary>
    /// Cancel past instances that fall on exception dates (for retroactively added exception dates)
    /// </summary>
    /// <param name="shiftId">Shift ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Number of instances cancelled</returns>
    [HttpPost("{shiftId}/cancel-past-exceptions")]
    public async Task<ActionResult<int>> CancelPastExceptionInstancesAsync(
        [FromRoute] string shiftId,
        [FromQuery] string reason = "Exception date added retroactively")
    {
        try
        {
            var count = await _shiftInstanceService.CancelPastExceptionInstancesAsync(shiftId, reason);
            _logger.LogInformation("Admin {UserId} cancelled {Count} past exception date instances for shift {ShiftId}",
                User.Identity?.Name, count, shiftId);
            return Ok(new { cancelled = count });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid shift ID: {ShiftId}", shiftId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling past exception date instances for shift {ShiftId}", shiftId);
            return StatusCode(500, "An error occurred while cancelling instances");
        }
    }

    /// <summary>
    /// Get current active shift instance for a specific shift
    /// </summary>
    /// <param name="shiftId">Shift ID</param>
    /// <returns>Current active shift instance if any</returns>
    [HttpGet("active/{shiftId}")]
    public async Task<ActionResult<ShiftInstanceResponse>> GetCurrentActiveInstanceAsync(
        [FromRoute] string shiftId)
    {
        try
        {
            var instance = await _shiftInstanceService.GetCurrentActiveInstanceAsync(shiftId);
            if (instance == null)
            {
                return NotFound($"No active shift instance found for shift {shiftId}");
            }

            return Ok(instance);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid shift ID provided: {ShiftId}", shiftId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active shift instance for shift {ShiftId}", shiftId);
            return StatusCode(500, "An error occurred while retrieving the active shift instance");
        }
    }
}

