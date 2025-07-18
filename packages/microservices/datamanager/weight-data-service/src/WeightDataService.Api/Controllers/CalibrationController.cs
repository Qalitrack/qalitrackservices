using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Weighbridge calibration management
/// </summary>
[ApiController]
[Route("api/calibration")]
public class CalibrationController : BaseController
{
    private readonly ICalibrationService _calibrationService;

    public CalibrationController(ICalibrationService calibrationService)
    {
        _calibrationService = calibrationService;
    }

    /// <summary>
    /// Create new calibration record
    /// </summary>
    [HttpPost("records")]
    public async Task<IActionResult> CreateCalibrationRecord([FromBody] CreateCalibrationRecordDto createDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var record = await _calibrationService.CreateCalibrationRecordAsync(createDto, organizationId, userId);
            
            return CreatedAtAction(nameof(GetCalibrationRecord), new { id = record.Id }, 
                Success(record, "Calibration record created successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CalibrationRecordDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get calibration record by ID
    /// </summary>
    [HttpGet("records/{id}")]
    public async Task<IActionResult> GetCalibrationRecord(Guid id)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var record = await _calibrationService.GetCalibrationRecordAsync(id, organizationId);
            
            if (record == null)
            {
                return NotFound(Error<CalibrationRecordDto>("Calibration record not found"));
            }

            return HandleResult(Success(record, "Calibration record retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CalibrationRecordDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get calibration history for weighbridge
    /// </summary>
    [HttpGet("weighbridge/{weighbridgeId}/history")]
    public async Task<IActionResult> GetCalibrationHistory(string weighbridgeId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var history = await _calibrationService.GetCalibrationHistoryAsync(weighbridgeId, organizationId);
            
            return HandleResult(Success(history, "Calibration history retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<CalibrationRecordDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get current calibration status for weighbridge
    /// </summary>
    [HttpGet("weighbridge/{weighbridgeId}/status")]
    public async Task<IActionResult> GetCalibrationStatus(string weighbridgeId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var status = await _calibrationService.GetCalibrationStatusAsync(weighbridgeId, organizationId);
            
            return HandleResult(Success(status, "Calibration status retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CalibrationStatusDto>(ex.Message));
        }
    }

    /// <summary>
    /// Detect calibration drift
    /// </summary>
    [HttpPost("weighbridge/{weighbridgeId}/drift-detection")]
    public async Task<IActionResult> DetectDrift(string weighbridgeId, [FromBody] DriftDetectionRequest request)
    {
        try
        {
            var drift = await _calibrationService.DetectDriftAsync(weighbridgeId, request.CurrentWeight, request.ReferenceWeight);
            
            return HandleResult(Success(drift, "Drift detection completed successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CalibrationDriftDto>(ex.Message));
        }
    }

    /// <summary>
    /// Perform automatic calibration
    /// </summary>
    [HttpPost("weighbridge/{weighbridgeId}/automatic")]
    public async Task<IActionResult> PerformAutomaticCalibration(string weighbridgeId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var success = await _calibrationService.PerformAutomaticCalibrationAsync(weighbridgeId, organizationId);
            
            return HandleResult(Success(success, "Automatic calibration completed successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }

    /// <summary>
    /// Get calibrations due for maintenance
    /// </summary>
    [HttpGet("due")]
    public async Task<IActionResult> GetCalibrationsDue()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var due = await _calibrationService.GetCalibrationsDueAsync(organizationId);
            
            return HandleResult(Success(due, "Due calibrations retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<CalibrationRecordDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Schedule calibration
    /// </summary>
    [HttpPost("weighbridge/{weighbridgeId}/schedule")]
    public async Task<IActionResult> ScheduleCalibration(string weighbridgeId, [FromBody] ScheduleCalibrationRequest request)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var scheduled = await _calibrationService.ScheduleCalibrationAsync(weighbridgeId, request.ScheduledDate, organizationId);
            
            return HandleResult(Success(scheduled, "Calibration scheduled successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }
}

public class DriftDetectionRequest
{
    public decimal CurrentWeight { get; set; }
    public decimal ReferenceWeight { get; set; }
}

public class ScheduleCalibrationRequest
{
    public DateTime ScheduledDate { get; set; }
}