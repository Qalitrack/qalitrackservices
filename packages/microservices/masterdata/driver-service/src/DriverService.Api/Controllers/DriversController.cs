using DriverService.Core.DTOs;
using DriverService.Core.Entities;
using DriverService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DriverService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;
    private readonly ILogger<DriversController> _logger;

    public DriversController(IDriverService driverService, ILogger<DriversController> logger)
    {
        _driverService = driverService;
        _logger = logger;
    }

    /// <summary>
    /// Get all drivers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DriverDto>>>> GetAllDrivers()
    {
        try
        {
            var drivers = await _driverService.GetAllDriversAsync();
            return Ok(ApiResponseDto<IEnumerable<DriverDto>>.SuccessResult(drivers, "Drivers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving drivers");
            return StatusCode(500, ApiResponseDto<IEnumerable<DriverDto>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get active drivers only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DriverDto>>>> GetActiveDrivers()
    {
        try
        {
            var drivers = await _driverService.GetActiveDriversAsync();
            return Ok(ApiResponseDto<IEnumerable<DriverDto>>.SuccessResult(drivers, "Active drivers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active drivers");
            return StatusCode(500, ApiResponseDto<IEnumerable<DriverDto>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get drivers by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DriverDto>>>> GetDriversByStatus(DriverStatus status)
    {
        try
        {
            var drivers = await _driverService.GetDriversByStatusAsync(status);
            return Ok(ApiResponseDto<IEnumerable<DriverDto>>.SuccessResult(drivers, $"Drivers with status {status} retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving drivers by status {Status}", status);
            return StatusCode(500, ApiResponseDto<IEnumerable<DriverDto>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Search drivers by term
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DriverDto>>>> SearchDrivers([FromQuery] string searchTerm)
    {
        try
        {
            var drivers = await _driverService.SearchDriversAsync(searchTerm);
            return Ok(ApiResponseDto<IEnumerable<DriverDto>>.SuccessResult(drivers, "Driver search completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching drivers with term {SearchTerm}", searchTerm);
            return StatusCode(500, ApiResponseDto<IEnumerable<DriverDto>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get drivers with expiring licenses
    /// </summary>
    [HttpGet("expiring-licenses")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DriverDto>>>> GetDriversWithExpiringLicenses([FromQuery] int daysAhead = 30)
    {
        try
        {
            var drivers = await _driverService.GetDriversWithExpiringLicensesAsync(daysAhead);
            return Ok(ApiResponseDto<IEnumerable<DriverDto>>.SuccessResult(drivers, $"Drivers with licenses expiring in {daysAhead} days retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving drivers with expiring licenses");
            return StatusCode(500, ApiResponseDto<IEnumerable<DriverDto>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get driver by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<DriverDto>>> GetDriverById(string id)
    {
        try
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            if (driver == null)
            {
                return NotFound(ApiResponseDto<DriverDto>.ErrorResult($"Driver with ID {id} not found"));
            }

            return Ok(ApiResponseDto<DriverDto>.SuccessResult(driver, "Driver retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving driver with ID {DriverId}", id);
            return StatusCode(500, ApiResponseDto<DriverDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get driver by employee ID
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<ApiResponseDto<DriverDto>>> GetDriverByEmployeeId(string employeeId)
    {
        try
        {
            var driver = await _driverService.GetDriverByEmployeeIdAsync(employeeId);
            if (driver == null)
            {
                return NotFound(ApiResponseDto<DriverDto>.ErrorResult($"Driver with employee ID {employeeId} not found"));
            }

            return Ok(ApiResponseDto<DriverDto>.SuccessResult(driver, "Driver retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving driver with employee ID {EmployeeId}", employeeId);
            return StatusCode(500, ApiResponseDto<DriverDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get driver by email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<ApiResponseDto<DriverDto>>> GetDriverByEmail(string email)
    {
        try
        {
            var driver = await _driverService.GetDriverByEmailAsync(email);
            if (driver == null)
            {
                return NotFound(ApiResponseDto<DriverDto>.ErrorResult($"Driver with email {email} not found"));
            }

            return Ok(ApiResponseDto<DriverDto>.SuccessResult(driver, "Driver retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving driver with email {Email}", email);
            return StatusCode(500, ApiResponseDto<DriverDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Create a new driver
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<DriverDto>>> CreateDriver([FromBody] CreateDriverDto createDriverDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<DriverDto>.ErrorResult(errors));
            }

            var driver = await _driverService.CreateDriverAsync(createDriverDto);
            return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, 
                ApiResponseDto<DriverDto>.SuccessResult(driver, "Driver created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating driver");
            return StatusCode(500, ApiResponseDto<DriverDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Update a driver
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<DriverDto>>> UpdateDriver(string id, [FromBody] UpdateDriverDto updateDriverDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<DriverDto>.ErrorResult(errors));
            }

            var driver = await _driverService.UpdateDriverAsync(id, updateDriverDto);
            return Ok(ApiResponseDto<DriverDto>.SuccessResult(driver, "Driver updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Driver not found for update: {DriverId}", id);
            return NotFound(ApiResponseDto<DriverDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating driver with ID {DriverId}", id);
            return StatusCode(500, ApiResponseDto<DriverDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Delete a driver (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteDriver(string id)
    {
        try
        {
            await _driverService.DeleteDriverAsync(id);
            return Ok(ApiResponseDto<object>.SuccessResult(new { }, "Driver deleted successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Driver not found for deletion: {DriverId}", id);
            return NotFound(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting driver with ID {DriverId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Register biometric data for a driver
    /// </summary>
    [HttpPost("{id}/biometric")]
    public async Task<ActionResult<ApiResponseDto<object>>> RegisterBiometric(string id, [FromBody] BiometricRegistrationDto biometricDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<object>.ErrorResult(errors));
            }

            await _driverService.RegisterBiometricAsync(id, biometricDto);
            return Ok(ApiResponseDto<object>.SuccessResult(new { }, "Biometric data registered successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Driver not found for biometric registration: {DriverId}", id);
            return NotFound(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering biometric data for driver {DriverId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Verify biometric data for a driver
    /// </summary>
    [HttpPost("{id}/biometric/verify")]
    public async Task<ActionResult<ApiResponseDto<BiometricVerificationResultDto>>> VerifyBiometric(string id, [FromBody] BiometricVerificationDto verificationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<BiometricVerificationResultDto>.ErrorResult(errors));
            }

            var result = await _driverService.VerifyBiometricAsync(id, verificationDto);
            return Ok(ApiResponseDto<BiometricVerificationResultDto>.SuccessResult(result, "Biometric verification completed"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Driver not found for biometric verification: {DriverId}", id);
            return NotFound(ApiResponseDto<BiometricVerificationResultDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying biometric data for driver {DriverId}", id);
            return StatusCode(500, ApiResponseDto<BiometricVerificationResultDto>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Update biometric settings for a driver
    /// </summary>
    [HttpPut("{id}/biometric/settings")]
    public async Task<ActionResult<ApiResponseDto<object>>> UpdateBiometricSettings(string id, [FromBody] BiometricSettingsDto settingsDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<object>.ErrorResult(errors));
            }

            await _driverService.UpdateBiometricSettingsAsync(id, settingsDto);
            return Ok(ApiResponseDto<object>.SuccessResult(new { }, "Biometric settings updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Driver not found for biometric settings update: {DriverId}", id);
            return NotFound(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating biometric settings for driver {DriverId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Internal server error"));
        }
    }
}