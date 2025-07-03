using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Weighbridge status management
/// </summary>
[ApiController]
[Route("api/weighbridges")]
public class WeighbridgeStatusController : BaseController
{
    private readonly IWeighbridgeStatusService _weighbridgeService;

    public WeighbridgeStatusController(IWeighbridgeStatusService weighbridgeService)
    {
        _weighbridgeService = weighbridgeService;
    }

    /// <summary>
    /// Get all weighbridges for organization
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWeighbridges()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var weighbridges = await _weighbridgeService.GetWeighbridgesAsync(organizationId);
            
            return HandleResult(Success(weighbridges, "Weighbridges retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeighbridgeStatusDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge status by ID
    /// </summary>
    [HttpGet("{weighbridgeId}/status")]
    public async Task<IActionResult> GetWeighbridgeStatus(string weighbridgeId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var weighbridge = await _weighbridgeService.GetWeighbridgeByIdAsync(weighbridgeId, organizationId);
            
            if (weighbridge == null)
            {
                return NotFound(Error<WeighbridgeStatusDto>("Weighbridge not found"));
            }

            return HandleResult(Success(weighbridge, "Weighbridge status retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeighbridgeStatusDto>(ex.Message));
        }
    }

    /// <summary>
    /// Create new weighbridge
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWeighbridge([FromBody] CreateWeighbridgeStatusDto createDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var weighbridge = await _weighbridgeService.CreateWeighbridgeAsync(createDto, organizationId, userId);
            
            return CreatedAtAction(nameof(GetWeighbridgeStatus), new { weighbridgeId = weighbridge.WeighbridgeId }, 
                Success(weighbridge, "Weighbridge created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return HandleResult(Error<WeighbridgeStatusDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeighbridgeStatusDto>(ex.Message));
        }
    }

    /// <summary>
    /// Update weighbridge status
    /// </summary>
    [HttpPut("{weighbridgeId}/status")]
    public async Task<IActionResult> UpdateWeighbridgeStatus(string weighbridgeId, [FromBody] UpdateWeighbridgeStatusDto updateDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var weighbridge = await _weighbridgeService.UpdateWeighbridgeStatusAsync(weighbridgeId, updateDto, organizationId, userId);
            
            if (weighbridge == null)
            {
                return NotFound(Error<WeighbridgeStatusDto>("Weighbridge not found"));
            }

            return HandleResult(Success(weighbridge, "Weighbridge status updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeighbridgeStatusDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get active weighbridges
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveWeighbridges()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var weighbridges = await _weighbridgeService.GetActiveWeighbridgesAsync(organizationId);
            
            return HandleResult(Success(weighbridges, "Active weighbridges retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeighbridgeStatusDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridges requiring calibration
    /// </summary>
    [HttpGet("calibration-due")]
    public async Task<IActionResult> GetWeighbridgesRequiringCalibration()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var weighbridges = await _weighbridgeService.GetWeighbridgesRequiringCalibrationAsync(organizationId);
            
            return HandleResult(Success(weighbridges, "Weighbridges requiring calibration retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeighbridgeStatusDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Update weighbridge current weight (for real-time updates)
    /// </summary>
    [HttpPut("{weighbridgeId}/current-weight")]
    public async Task<IActionResult> UpdateCurrentWeight(string weighbridgeId, [FromBody] decimal currentWeight)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var weighbridge = await _weighbridgeService.UpdateCurrentWeightAsync(weighbridgeId, currentWeight, organizationId);
            
            if (weighbridge == null)
            {
                return NotFound(Error<WeighbridgeStatusDto>("Weighbridge not found"));
            }

            return HandleResult(Success(weighbridge, "Current weight updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeighbridgeStatusDto>(ex.Message));
        }
    }
}