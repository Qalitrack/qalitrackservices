using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Weight corrections management
/// </summary>
[ApiController]
[Route("api/corrections")]
public class WeightCorrectionsController : BaseController
{
    private readonly IWeightCorrectionService _correctionService;

    public WeightCorrectionsController(IWeightCorrectionService correctionService)
    {
        _correctionService = correctionService;
    }

    /// <summary>
    /// Create weight correction
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCorrection([FromBody] CreateWeightCorrectionDto createDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var correction = await _correctionService.CreateCorrectionAsync(createDto, organizationId, userId);
            
            return CreatedAtAction(nameof(GetCorrectionsByMeasurement), new { measurementId = correction.WeightMeasurementId }, 
                Success(correction, "Weight correction created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return HandleResult(Error<WeightCorrectionDto>(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeightCorrectionDto>(ex.Message));
        }
    }

    /// <summary>
    /// Approve or reject weight correction
    /// </summary>
    [HttpPut("{correctionId}/approve")]
    public async Task<IActionResult> ApproveCorrection(Guid correctionId, [FromBody] ApproveWeightCorrectionDto approveDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var correction = await _correctionService.ApproveCorrectionAsync(correctionId, approveDto, organizationId, userId);
            
            if (correction == null)
            {
                return NotFound(Error<WeightCorrectionDto>("Correction not found"));
            }

            return HandleResult(Success(correction, "Weight correction approved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeightCorrectionDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get corrections by measurement ID
    /// </summary>
    [HttpGet("measurement/{measurementId}")]
    public async Task<IActionResult> GetCorrectionsByMeasurement(Guid measurementId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var corrections = await _correctionService.GetCorrectionsByMeasurementAsync(measurementId, organizationId);
            
            return HandleResult(Success(corrections, "Corrections retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeightCorrectionDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get pending corrections requiring approval
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingCorrections()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var corrections = await _correctionService.GetPendingCorrectionsAsync(organizationId);
            
            return HandleResult(Success(corrections, "Pending corrections retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeightCorrectionDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get recent corrections
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentCorrections([FromQuery] int days = 30)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var corrections = await _correctionService.GetRecentCorrectionsAsync(organizationId, days);
            
            return HandleResult(Success(corrections, "Recent corrections retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeightCorrectionDto>>(ex.Message));
        }
    }
}