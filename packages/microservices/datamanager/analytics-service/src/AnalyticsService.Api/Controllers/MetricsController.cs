using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Core.Interfaces;
using AnalyticsService.Core.DTOs;

namespace AnalyticsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IAnalyticsEngine _analyticsEngine;

    public MetricsController(IAnalyticsEngine analyticsEngine)
    {
        _analyticsEngine = analyticsEngine;
    }

    /// <summary>
    /// Get operational metrics for an organization
    /// </summary>
    [HttpGet("operational")]
    public async Task<ActionResult<MetricResultCollection>> GetOperationalMetrics(
        [FromQuery] string organizationId,
        [FromQuery] string timeRange = "24h",
        [FromQuery] string? weighbridgeId = null)
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            var metricTypes = new List<string> 
            { 
                "TransactionVolume", 
                "ProcessingTime", 
                "Throughput", 
                "QueueTime" 
            };

            if (!string.IsNullOrEmpty(weighbridgeId))
            {
                metricTypes.Add("WeighbridgeUtilization");
            }

            var metrics = await _analyticsEngine.GetAggregatedMetricsAsync(organizationId, metricTypes, range, "current");
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get performance indicators for an organization
    /// </summary>
    [HttpGet("performance")]
    public async Task<ActionResult<MetricResultCollection>> GetPerformanceMetrics(
        [FromQuery] string organizationId,
        [FromQuery] string timeRange = "24h")
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            var metricTypes = new List<string> 
            { 
                "WeighbridgeUtilization", 
                "ProcessingTime", 
                "Throughput",
                "QueueTime",
                "Accuracy"
            };

            var metrics = await _analyticsEngine.GetAggregatedMetricsAsync(organizationId, metricTypes, range, "average");
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get financial metrics for an organization
    /// </summary>
    [HttpGet("financial")]
    public async Task<ActionResult<MetricResult>> GetFinancialMetrics(
        [FromQuery] string organizationId,
        [FromQuery] string timeRange = "30d")
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            var revenue = await _analyticsEngine.CalculateRevenueTotalAsync(organizationId, range);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get compliance metrics for an organization
    /// </summary>
    [HttpGet("compliance")]
    public async Task<ActionResult<MetricResult>> GetComplianceMetrics(
        [FromQuery] string organizationId,
        [FromQuery] string timeRange = "30d")
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            var compliance = await _analyticsEngine.CalculateComplianceRateAsync(organizationId, range);
            return Ok(compliance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get real-time metrics for an organization
    /// </summary>
    [HttpGet("real-time")]
    public async Task<ActionResult<MetricResultCollection>> GetRealTimeMetrics(
        [FromQuery] string organizationId)
    {
        try
        {
            var metrics = await _analyticsEngine.GetRealTimeMetricsAsync(organizationId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get time series data for a specific metric
    /// </summary>
    [HttpGet("time-series")]
    public async Task<ActionResult<TimeSeriesResult>> GetTimeSeries(
        [FromQuery] string organizationId,
        [FromQuery] string metricType,
        [FromQuery] string timeRange = "7d",
        [FromQuery] string granularity = "hour",
        [FromQuery] string? weighbridgeId = null)
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            var timeSeries = await _analyticsEngine.GetMetricTimeSeriesAsync(organizationId, metricType, range, granularity, weighbridgeId);
            return Ok(timeSeries);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific metric by type
    /// </summary>
    [HttpGet("{metricType}")]
    public async Task<ActionResult<MetricResult>> GetMetric(
        string metricType,
        [FromQuery] string organizationId,
        [FromQuery] string timeRange = "24h",
        [FromQuery] string? weighbridgeId = null)
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            
            var metric = metricType.ToLower() switch
            {
                "transactionvolume" => await _analyticsEngine.CalculateTransactionVolumeAsync(organizationId, range, weighbridgeId),
                "processingtime" => await _analyticsEngine.CalculateAverageProcessingTimeAsync(organizationId, range, weighbridgeId),
                "weighbridgeutilization" when !string.IsNullOrEmpty(weighbridgeId) => await _analyticsEngine.CalculateWeighbridgeUtilizationAsync(weighbridgeId, range),
                "revenue" => await _analyticsEngine.CalculateRevenueTotalAsync(organizationId, range),
                "compliancerate" => await _analyticsEngine.CalculateComplianceRateAsync(organizationId, range),
                _ => throw new ArgumentException($"Unknown metric type: {metricType}")
            };

            return Ok(metric);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}