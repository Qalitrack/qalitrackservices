using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Historical weight data analysis
/// </summary>
[ApiController]
[Route("api/analysis")]
public class HistoricalAnalysisController : BaseController
{
    private readonly IHistoricalAnalysisService _analysisService;

    public HistoricalAnalysisController(IHistoricalAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    /// <summary>
    /// Create new historical analysis
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAnalysis([FromBody] CreateHistoricalAnalysisDto createDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var analysis = await _analysisService.CreateAnalysisAsync(createDto, organizationId, userId);
            
            return CreatedAtAction(nameof(GetAnalysis), new { id = analysis.Id }, 
                Success(analysis, "Historical analysis created successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<HistoricalAnalysisDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get historical analysis by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysis(Guid id)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var analysis = await _analysisService.GetAnalysisAsync(id, organizationId);
            
            if (analysis == null)
            {
                return NotFound(Error<HistoricalAnalysisDto>("Historical analysis not found"));
            }

            return HandleResult(Success(analysis, "Historical analysis retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<HistoricalAnalysisDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get historical analyses with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAnalyses([FromQuery] string? weighbridgeId = null, [FromQuery] string? vehicleRegistration = null)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var analyses = await _analysisService.GetAnalysesAsync(organizationId, weighbridgeId, vehicleRegistration);
            
            return HandleResult(Success(analyses, "Historical analyses retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<HistoricalAnalysisDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get trend analysis for weighbridge
    /// </summary>
    [HttpGet("trends/weighbridge/{weighbridgeId}")]
    public async Task<IActionResult> GetTrendAnalysis(string weighbridgeId, 
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate,
        [FromQuery] string? vehicleRegistration = null, [FromQuery] string? productType = null)
    {
        try
        {
            var trend = await _analysisService.GetTrendAnalysisAsync(weighbridgeId, fromDate, toDate, vehicleRegistration, productType);
            
            return HandleResult(Success(trend, "Trend analysis retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<TrendAnalysisDto>(ex.Message));
        }
    }

    /// <summary>
    /// Detect anomalies in weight data
    /// </summary>
    [HttpGet("anomalies/weighbridge/{weighbridgeId}")]
    public async Task<IActionResult> DetectAnomalies(string weighbridgeId, 
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] decimal threshold = 2.0m)
    {
        try
        {
            var anomalies = await _analysisService.DetectAnomaliesAsync(weighbridgeId, fromDate, toDate, threshold);
            
            return HandleResult(Success(anomalies, "Anomalies detected successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<AnomalyDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Schedule automatic analysis
    /// </summary>
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleAutomaticAnalysis([FromBody] ScheduleAnalysisRequest request)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var scheduled = await _analysisService.ScheduleAutomaticAnalysisAsync(request.WeighbridgeId, request.AnalysisType, organizationId);
            
            return HandleResult(Success(scheduled, "Automatic analysis scheduled successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }

    /// <summary>
    /// Get weight statistics for weighbridge
    /// </summary>
    [HttpGet("statistics/weighbridge/{weighbridgeId}")]
    public async Task<IActionResult> GetWeightStatistics(string weighbridgeId, 
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var statistics = await _analysisService.GetWeightStatisticsAsync(weighbridgeId, fromDate, toDate, organizationId);
            
            return HandleResult(Success(statistics, "Weight statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<Dictionary<string, decimal>>(ex.Message));
        }
    }
}

public class ScheduleAnalysisRequest
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public WeightDataService.Core.Entities.AnalysisType AnalysisType { get; set; }
}