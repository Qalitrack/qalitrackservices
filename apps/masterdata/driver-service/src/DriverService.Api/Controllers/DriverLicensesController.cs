using DriverService.Core.DTOs;
using DriverService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DriverService.Api.Controllers;

[ApiController]
[Route("api/drivers/{driverId}/license")]
public class DriverLicensesController : ControllerBase
{
    private readonly IDriverService _driverService;
    private readonly ILogger<DriverLicensesController> _logger;

    public DriverLicensesController(IDriverService driverService, ILogger<DriverLicensesController> logger)
    {
        _driverService = driverService;
        _logger = logger;
    }

    /// <summary>
    /// Get driver license
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<DriverLicenseDto>>> GetDriverLicense(string driverId)
    {
        try
        {
            var license = await _driverService.GetDriverLicenseAsync(driverId);
            if (license == null)
            {
                return NotFound(ApiResponseDto<DriverLicenseDto>.ErrorResult($"License for driver {driverId} not found"));
            }

            return Ok(ApiResponseDto<DriverLicenseDto>.SuccessResult(license, "Driver license retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving license for driver {DriverId}", driverId);
            return StatusCode(500, ApiResponseDto<DriverLicenseDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Create driver license
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<DriverLicenseDto>>> CreateDriverLicense(
        string driverId, 
        [FromBody] CreateDriverLicenseDto createLicenseDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<DriverLicenseDto>.ErrorResult(errors));
            }

            // Ensure the driver ID matches
            createLicenseDto.DriverId = driverId;

            var license = await _driverService.CreateDriverLicenseAsync(createLicenseDto);
            return CreatedAtAction(nameof(GetDriverLicense), new { driverId = driverId }, 
                ApiResponseDto<DriverLicenseDto>.SuccessResult(license, "Driver license created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating license for driver {DriverId}", driverId);
            return StatusCode(500, ApiResponseDto<DriverLicenseDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Update driver license
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponseDto<DriverLicenseDto>>> UpdateDriverLicense(
        string driverId, 
        [FromBody] UpdateDriverLicenseDto updateLicenseDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<DriverLicenseDto>.ErrorResult(errors));
            }

            var license = await _driverService.UpdateDriverLicenseAsync(driverId, updateLicenseDto);
            return Ok(ApiResponseDto<DriverLicenseDto>.SuccessResult(license, "Driver license updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "License not found for driver: {DriverId}", driverId);
            return NotFound(ApiResponseDto<DriverLicenseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating license for driver {DriverId}", driverId);
            return StatusCode(500, ApiResponseDto<DriverLicenseDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Validate driver license
    /// </summary>
    [HttpGet("validate")]
    public async Task<ActionResult<ApiResponseDto<LicenseValidationResult>>> ValidateDriverLicense(string driverId)
    {
        try
        {
            var validationResult = await _driverService.ValidateLicenseAsync(driverId);
            return Ok(ApiResponseDto<LicenseValidationResult>.SuccessResult(validationResult, "License validation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license for driver {DriverId}", driverId);
            return StatusCode(500, ApiResponseDto<LicenseValidationResult>.ErrorResult("Internal server error"));
        }
    }
}