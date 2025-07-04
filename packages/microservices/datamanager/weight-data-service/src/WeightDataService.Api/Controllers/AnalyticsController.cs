using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Analytics and reporting
/// </summary>
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get analytics summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetAnalyticsSummary([FromQuery] AnalyticsRequestDto request)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var summary = await _analyticsService.GetAnalyticsSummaryAsync(organizationId, request);
            
            return HandleResult(Success(summary, "Analytics summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<AnalyticsSummaryDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge usage statistics
    /// </summary>
    [HttpGet("weighbridge-usage")]
    public async Task<IActionResult> GetWeighbridgeUsage([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var usage = await _analyticsService.GetWeighbridgeUsageAsync(organizationId, fromDate, toDate);
            
            return HandleResult(Success(usage, "Weighbridge usage retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeighbridgeUsageDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get daily weight summary
    /// </summary>
    [HttpGet("daily-weights")]
    public async Task<IActionResult> GetDailyWeightSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var dailyWeights = await _analyticsService.GetDailyWeightSummaryAsync(organizationId, fromDate, toDate);
            
            return HandleResult(Success(dailyWeights, "Daily weight summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<DailyWeightDto>>(ex.Message));
        }
    }
}