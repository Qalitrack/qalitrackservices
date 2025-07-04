using Microsoft.AspNetCore.Mvc;
using WeighbridgeService.Core.DTOs;
using WeighbridgeService.Core.Interfaces;

namespace WeighbridgeService.Api.Controllers;

[ApiController]
[Route("api/weighbridges/{weighbridgeId}/calibration")]
public class WeighbridgeCalibrationController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<WeighbridgeCalibrationController> _logger;

    public WeighbridgeCalibrationController(IWeighbridgeService weighbridgeService, ILogger<WeighbridgeCalibrationController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>>> GetCalibrationHistory(string weighbridgeId)
    {
        try
        {
            var calibrations = await _weighbridgeService.GetCalibrationHistoryAsync(weighbridgeId);
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>.SuccessResponse(calibrations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calibration history for weighbridge {WeighbridgeId}", weighbridgeId);
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>.ErrorResponse("An error occurred while retrieving calibration history"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeCalibrationDto>>> ScheduleCalibration(
        string weighbridgeId, [FromBody] ScheduleCalibrationRequest request)
    {
        try
        {
            var calibration = await _weighbridgeService.ScheduleCalibrationAsync(weighbridgeId, request);
            return CreatedAtAction(nameof(GetCalibrationHistory), new { weighbridgeId }, 
                ApiResponseDto<WeighbridgeCalibrationDto>.SuccessResponse(calibration, "Calibration scheduled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling calibration for weighbridge {WeighbridgeId}", weighbridgeId);
            return StatusCode(500, ApiResponseDto<WeighbridgeCalibrationDto>.ErrorResponse("An error occurred while scheduling calibration"));
        }
    }
}

[ApiController]
[Route("api/calibrations")]
public class CalibrationsController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<CalibrationsController> _logger;

    public CalibrationsController(IWeighbridgeService weighbridgeService, ILogger<CalibrationsController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpPut("{calibrationId}")]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeCalibrationDto>>> RecordCalibration(
        string calibrationId, [FromBody] RecordCalibrationRequest request)
    {
        try
        {
            var calibration = await _weighbridgeService.RecordCalibrationAsync(calibrationId, request);
            return Ok(ApiResponseDto<WeighbridgeCalibrationDto>.SuccessResponse(calibration, "Calibration recorded successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponseDto<WeighbridgeCalibrationDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording calibration {CalibrationId}", calibrationId);
            return StatusCode(500, ApiResponseDto<WeighbridgeCalibrationDto>.ErrorResponse("An error occurred while recording calibration"));
        }
    }

    [HttpGet("scheduled")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>>> GetScheduledCalibrations()
    {
        try
        {
            var calibrations = await _weighbridgeService.GetScheduledCalibrationsAsync();
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>.SuccessResponse(calibrations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scheduled calibrations");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>.ErrorResponse("An error occurred while retrieving scheduled calibrations"));
        }
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>>> GetOverdueCalibrations()
    {
        try
        {
            var calibrations = await _weighbridgeService.GetOverdueCalibrationsAsync();
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>.SuccessResponse(calibrations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue calibrations");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeCalibrationDto>>.ErrorResponse("An error occurred while retrieving overdue calibrations"));
        }
    }
}