using Microsoft.AspNetCore.Mvc;
using WeighbridgeService.Core.DTOs;
using WeighbridgeService.Core.Interfaces;

namespace WeighbridgeService.Api.Controllers;

[ApiController]
[Route("api/weighbridges/{weighbridgeId}/maintenance")]
public class WeighbridgeMaintenanceController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<WeighbridgeMaintenanceController> _logger;

    public WeighbridgeMaintenanceController(IWeighbridgeService weighbridgeService, ILogger<WeighbridgeMaintenanceController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>>> GetMaintenanceSchedule(string weighbridgeId)
    {
        try
        {
            var maintenance = await _weighbridgeService.GetMaintenanceScheduleAsync(weighbridgeId);
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>.SuccessResponse(maintenance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance schedule for weighbridge {WeighbridgeId}", weighbridgeId);
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>.ErrorResponse("An error occurred while retrieving maintenance schedule"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeMaintenanceDto>>> ScheduleMaintenance(
        string weighbridgeId, [FromBody] ScheduleMaintenanceRequest request)
    {
        try
        {
            var maintenance = await _weighbridgeService.ScheduleMaintenanceAsync(weighbridgeId, request);
            return CreatedAtAction(nameof(GetMaintenanceSchedule), new { weighbridgeId }, 
                ApiResponseDto<WeighbridgeMaintenanceDto>.SuccessResponse(maintenance, "Maintenance scheduled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling maintenance for weighbridge {WeighbridgeId}", weighbridgeId);
            return StatusCode(500, ApiResponseDto<WeighbridgeMaintenanceDto>.ErrorResponse("An error occurred while scheduling maintenance"));
        }
    }
}

[ApiController]
[Route("api/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(IWeighbridgeService weighbridgeService, ILogger<MaintenanceController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpPut("{maintenanceId}")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeMaintenanceDto>>> UpdateMaintenance(
        string maintenanceId, [FromBody] UpdateMaintenanceRequest request)
    {
        try
        {
            var maintenance = await _weighbridgeService.UpdateMaintenanceAsync(maintenanceId, request);
            return Ok(ApiResponseDto<WeighbridgeMaintenanceDto>.SuccessResponse(maintenance, "Maintenance updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponseDto<WeighbridgeMaintenanceDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance {MaintenanceId}", maintenanceId);
            return StatusCode(500, ApiResponseDto<WeighbridgeMaintenanceDto>.ErrorResponse("An error occurred while updating maintenance"));
        }
    }

    [HttpGet("scheduled")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>>> GetScheduledMaintenance()
    {
        try
        {
            var maintenance = await _weighbridgeService.GetScheduledMaintenanceAsync();
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>.SuccessResponse(maintenance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scheduled maintenance");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>.ErrorResponse("An error occurred while retrieving scheduled maintenance"));
        }
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>>> GetOverdueMaintenance()
    {
        try
        {
            var maintenance = await _weighbridgeService.GetOverdueMaintenanceAsync();
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>.SuccessResponse(maintenance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue maintenance");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeMaintenanceDto>>.ErrorResponse("An error occurred while retrieving overdue maintenance"));
        }
    }
}