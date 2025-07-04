using DriverService.Core.DTOs;
using DriverService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DriverService.Api.Controllers;

[ApiController]
[Route("api/drivers/{driverId}/violations")]
public class DriverViolationsController : ControllerBase
{
    private readonly IDriverService _driverService;
    private readonly ILogger<DriverViolationsController> _logger;

    public DriverViolationsController(IDriverService driverService, ILogger<DriverViolationsController> logger)
    {
        _driverService = driverService;
        _logger = logger;
    }

    /// <summary>
    /// Get all violations for a driver
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DriverViolationDto>>>> GetDriverViolations(string driverId)
    {
        try
        {
            var violations = await _driverService.GetDriverViolationsAsync(driverId);
            return Ok(ApiResponseDto<IEnumerable<DriverViolationDto>>.SuccessResult(violations, "Driver violations retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving violations for driver {DriverId}", driverId);
            return StatusCode(500, ApiResponseDto<IEnumerable<DriverViolationDto>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Record a new violation for a driver
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<DriverViolationDto>>> AddDriverViolation(
        string driverId, 
        [FromBody] CreateDriverViolationDto createViolationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<DriverViolationDto>.ErrorResult(errors));
            }

            // Ensure the driver ID matches
            createViolationDto.DriverId = driverId;

            var violation = await _driverService.AddDriverViolationAsync(createViolationDto);
            return CreatedAtAction(nameof(GetDriverViolations), new { driverId = driverId }, 
                ApiResponseDto<DriverViolationDto>.SuccessResult(violation, "Driver violation recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording violation for driver {DriverId}", driverId);
            return StatusCode(500, ApiResponseDto<DriverViolationDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Update a specific violation
    /// </summary>
    [HttpPut("{violationId}")]
    public async Task<ActionResult<ApiResponseDto<DriverViolationDto>>> UpdateDriverViolation(
        string driverId,
        string violationId, 
        [FromBody] UpdateDriverViolationDto updateViolationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<DriverViolationDto>.ErrorResult(errors));
            }

            var violation = await _driverService.UpdateDriverViolationAsync(violationId, updateViolationDto);
            return Ok(ApiResponseDto<DriverViolationDto>.SuccessResult(violation, "Driver violation updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Violation not found: {ViolationId}", violationId);
            return NotFound(ApiResponseDto<DriverViolationDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating violation {ViolationId} for driver {DriverId}", violationId, driverId);
            return StatusCode(500, ApiResponseDto<DriverViolationDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Delete a specific violation
    /// </summary>
    [HttpDelete("{violationId}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteDriverViolation(string driverId, string violationId)
    {
        try
        {
            await _driverService.DeleteDriverViolationAsync(violationId);
            return Ok(ApiResponseDto<object>.SuccessResult(new { }, "Driver violation deleted successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Violation not found for deletion: {ViolationId}", violationId);
            return NotFound(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting violation {ViolationId} for driver {DriverId}", violationId, driverId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Internal server error"));
        }
    }
}