using Microsoft.AspNetCore.Mvc;
using WeighbridgeService.Core.DTOs;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;

namespace WeighbridgeService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeighbridgesController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<WeighbridgesController> _logger;

    public WeighbridgesController(IWeighbridgeService weighbridgeService, ILogger<WeighbridgesController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeDto>>>> GetWeighbridges(
        [FromQuery] WeighbridgeStatus? status = null)
    {
        try
        {
            IEnumerable<WeighbridgeDto> weighbridges;
            
            if (status.HasValue)
            {
                weighbridges = await _weighbridgeService.GetWeighbridgesByStatusAsync(status.Value);
            }
            else
            {
                weighbridges = await _weighbridgeService.GetAllWeighbridgesAsync();
            }

            return Ok(ApiResponseDto<IEnumerable<WeighbridgeDto>>.SuccessResponse(weighbridges));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weighbridges");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeDto>>.ErrorResponse("An error occurred while retrieving weighbridges"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeDto>>> GetWeighbridge(string id)
    {
        try
        {
            var weighbridge = await _weighbridgeService.GetWeighbridgeByIdAsync(id);
            if (weighbridge == null)
            {
                return NotFound(ApiResponseDto<WeighbridgeDto>.ErrorResponse("Weighbridge not found"));
            }

            return Ok(ApiResponseDto<WeighbridgeDto>.SuccessResponse(weighbridge));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeDto>.ErrorResponse("An error occurred while retrieving the weighbridge"));
        }
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeDto>>> GetWeighbridgeByCode(string code)
    {
        try
        {
            var weighbridge = await _weighbridgeService.GetWeighbridgeByCodeAsync(code);
            if (weighbridge == null)
            {
                return NotFound(ApiResponseDto<WeighbridgeDto>.ErrorResponse("Weighbridge not found"));
            }

            return Ok(ApiResponseDto<WeighbridgeDto>.SuccessResponse(weighbridge));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weighbridge with code {Code}", code);
            return StatusCode(500, ApiResponseDto<WeighbridgeDto>.ErrorResponse("An error occurred while retrieving the weighbridge"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeDto>>> RegisterWeighbridge([FromBody] RegisterWeighbridgeRequest request)
    {
        try
        {
            var weighbridge = await _weighbridgeService.RegisterWeighbridgeAsync(request);
            return CreatedAtAction(nameof(GetWeighbridge), new { id = weighbridge.Id }, 
                ApiResponseDto<WeighbridgeDto>.SuccessResponse(weighbridge, "Weighbridge registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering weighbridge");
            return StatusCode(500, ApiResponseDto<WeighbridgeDto>.ErrorResponse("An error occurred while registering the weighbridge"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeDto>>> UpdateWeighbridge(string id, [FromBody] UpdateWeighbridgeRequest request)
    {
        try
        {
            var weighbridge = await _weighbridgeService.UpdateWeighbridgeAsync(id, request);
            return Ok(ApiResponseDto<WeighbridgeDto>.SuccessResponse(weighbridge, "Weighbridge updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponseDto<WeighbridgeDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeDto>.ErrorResponse("An error occurred while updating the weighbridge"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteWeighbridge(string id)
    {
        try
        {
            await _weighbridgeService.DeleteWeighbridgeAsync(id);
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Weighbridge deleted successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponseDto<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred while deleting the weighbridge"));
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeDto>>>> GetActiveWeighbridges()
    {
        try
        {
            var weighbridges = await _weighbridgeService.GetActiveWeighbridgesAsync();
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeDto>>.SuccessResponse(weighbridges));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active weighbridges");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeDto>>.ErrorResponse("An error occurred while retrieving active weighbridges"));
        }
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeDto>>>> GetAvailableWeighbridges(
        [FromQuery] DateTime? requestedTime = null)
    {
        try
        {
            var time = requestedTime ?? DateTime.Now;
            var weighbridges = await _weighbridgeService.GetAvailableWeighbridgesAsync(time);
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeDto>>.SuccessResponse(weighbridges));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available weighbridges");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeDto>>.ErrorResponse("An error occurred while retrieving available weighbridges"));
        }
    }

    // Configuration endpoints
    [HttpGet("{id}/configuration")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeConfigurationDto>>> GetConfiguration(string id)
    {
        try
        {
            var configuration = await _weighbridgeService.GetConfigurationAsync(id);
            if (configuration == null)
            {
                return NotFound(ApiResponseDto<WeighbridgeConfigurationDto>.ErrorResponse("Configuration not found"));
            }

            return Ok(ApiResponseDto<WeighbridgeConfigurationDto>.SuccessResponse(configuration));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration for weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeConfigurationDto>.ErrorResponse("An error occurred while retrieving the configuration"));
        }
    }

    [HttpPut("{id}/configuration")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeConfigurationDto>>> UpdateConfiguration(
        string id, [FromBody] UpdateWeighbridgeConfigurationRequest request)
    {
        try
        {
            var configuration = await _weighbridgeService.UpdateConfigurationAsync(id, request);
            return Ok(ApiResponseDto<WeighbridgeConfigurationDto>.SuccessResponse(configuration, "Configuration updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration for weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeConfigurationDto>.ErrorResponse("An error occurred while updating the configuration"));
        }
    }

    // Capacity endpoints
    [HttpGet("{id}/capacity")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeCapacityDto>>> GetCurrentCapacity(string id)
    {
        try
        {
            var capacity = await _weighbridgeService.GetCurrentCapacityAsync(id);
            if (capacity == null)
            {
                return NotFound(ApiResponseDto<WeighbridgeCapacityDto>.ErrorResponse("Capacity information not found"));
            }

            return Ok(ApiResponseDto<WeighbridgeCapacityDto>.SuccessResponse(capacity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving capacity for weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeCapacityDto>.ErrorResponse("An error occurred while retrieving capacity information"));
        }
    }

    [HttpPut("{id}/capacity")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeCapacityDto>>> UpdateCapacity(
        string id, [FromBody] UpdateCapacityRequest request)
    {
        try
        {
            var capacity = await _weighbridgeService.UpdateCapacityAsync(id, request);
            return Ok(ApiResponseDto<WeighbridgeCapacityDto>.SuccessResponse(capacity, "Capacity updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponseDto<WeighbridgeCapacityDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating capacity for weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeCapacityDto>.ErrorResponse("An error occurred while updating capacity"));
        }
    }

    // Location endpoints
    [HttpGet("{id}/location")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeLocationDto>>> GetLocation(string id)
    {
        try
        {
            var location = await _weighbridgeService.GetLocationAsync(id);
            if (location == null)
            {
                return NotFound(ApiResponseDto<WeighbridgeLocationDto>.ErrorResponse("Location not found"));
            }

            return Ok(ApiResponseDto<WeighbridgeLocationDto>.SuccessResponse(location));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location for weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeLocationDto>.ErrorResponse("An error occurred while retrieving location"));
        }
    }

    [HttpPut("{id}/location")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeLocationDto>>> UpdateLocation(
        string id, [FromBody] CreateWeighbridgeLocationRequest request)
    {
        try
        {
            var location = await _weighbridgeService.UpdateLocationAsync(id, request);
            return Ok(ApiResponseDto<WeighbridgeLocationDto>.SuccessResponse(location, "Location updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location for weighbridge {Id}", id);
            return StatusCode(500, ApiResponseDto<WeighbridgeLocationDto>.ErrorResponse("An error occurred while updating location"));
        }
    }
}