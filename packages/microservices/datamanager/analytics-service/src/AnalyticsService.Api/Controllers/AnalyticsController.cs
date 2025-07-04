using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Core.Interfaces;
using AnalyticsService.Core.DTOs;

namespace AnalyticsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsEngine _analyticsEngine;
    private readonly ITrendAnalysisService _trendAnalysisService;
    private readonly IAnomalyDetectionService _anomalyDetectionService;
    private readonly IForecastingService _forecastingService;

    public AnalyticsController(
        IAnalyticsEngine analyticsEngine,
        ITrendAnalysisService trendAnalysisService,
        IAnomalyDetectionService anomalyDetectionService,
        IForecastingService forecastingService)
    {
        _analyticsEngine = analyticsEngine;
        _trendAnalysisService = trendAnalysisService;
        _anomalyDetectionService = anomalyDetectionService;
        _forecastingService = forecastingService;
    }

    /// <summary>
    /// Get trend analysis for a specific metric
    /// </summary>
    [HttpGet("trends/{metricType}")]
    public async Task<ActionResult<TrendAnalysisResponse>> GetTrendAnalysis(
        string metricType,
        [FromQuery] string organizationId,
        [FromQuery] string timeRange = "30d",
        [FromQuery] string? weighbridgeId = null)
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            var trendAnalysis = await _analyticsEngine.AnalyzeTrendsAsync(organizationId, metricType, range, weighbridgeId);
            return Ok(trendAnalysis);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get forecasts for a specific metric
    /// </summary>
    [HttpGet("forecasts/{metricType}")]
    public async Task<ActionResult<ForecastResult>> GetForecast(
        string metricType,
        [FromQuery] string organizationId,
        [FromQuery] int days = 7,
        [FromQuery] string? weighbridgeId = null)
    {
        try
        {
            var forecast = await _analyticsEngine.GenerateForecastAsync(organizationId, metricType, days, weighbridgeId);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get anomaly detection results
    /// </summary>
    [HttpGet("anomalies")]
    public async Task<ActionResult<List<AnomalyResponse>>> GetAnomalies(
        [FromQuery] string organizationId,
        [FromQuery] string? metricType = null,
        [FromQuery] string timeRange = "7d",
        [FromQuery] string? severity = null)
    {
        try
        {
            var range = TimeRange.FromString(timeRange);
            
            if (!string.IsNullOrEmpty(metricType))
            {
                var anomalies = await _analyticsEngine.DetectAnomaliesAsync(organizationId, metricType, range);
                
                if (!string.IsNullOrEmpty(severity))
                {
                    anomalies = anomalies.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                return Ok(anomalies);
            }
            else
            {
                // Get anomalies for all metric types
                var allMetricTypes = new List<string> { "TransactionVolume", "ProcessingTime", "Revenue", "ComplianceRate" };
                var allAnomalies = new List<AnomalyResponse>();
                
                foreach (var metric in allMetricTypes)
                {
                    var anomalies = await _analyticsEngine.DetectAnomaliesAsync(organizationId, metric, range);
                    allAnomalies.AddRange(anomalies);
                }
                
                if (!string.IsNullOrEmpty(severity))
                {
                    allAnomalies = allAnomalies.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                return Ok(allAnomalies.OrderByDescending(a => a.Timestamp).ToList());
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Update anomaly status
    /// </summary>
    [HttpPut("anomalies/{anomalyId}/status")]
    public async Task<ActionResult> UpdateAnomalyStatus(
        Guid anomalyId,
        [FromBody] UpdateAnomalyStatusRequest request)
    {
        try
        {
            await _anomalyDetectionService.UpdateAnomalyStatusAsync(anomalyId, request.Status, request.Resolution, request.ResolvedBy);
            return Ok(new { message = "Anomaly status updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get benchmark comparisons
    /// </summary>
    [HttpGet("benchmarks")]
    public async Task<ActionResult<List<BenchmarkComparison>>> GetBenchmarkComparisons(
        [FromQuery] string organizationId,
        [FromQuery] string[]? metricTypes = null)
    {
        try
        {
            var metrics = metricTypes?.ToList() ?? new List<string> 
            { 
                "TransactionVolume", 
                "ProcessingTime", 
                "Revenue", 
                "ComplianceRate" 
            };
            
            var comparisons = await _analyticsEngine.GetBenchmarkComparisonsAsync(organizationId, metrics);
            return Ok(comparisons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Compare multiple organizations or weighbridges
    /// </summary>
    [HttpPost("compare")]
    public async Task<ActionResult<Dictionary<string, MetricResultCollection>>> CompareMetrics(
        [FromBody] CompareMetricsRequest request)
    {
        try
        {
            var results = new Dictionary<string, MetricResultCollection>();
            var range = TimeRange.FromString(request.TimeRange);

            foreach (var entityId in request.EntityIds)
            {
                MetricResultCollection metrics;
                
                if (request.ComparisonType.Equals("organization", StringComparison.OrdinalIgnoreCase))
                {
                    metrics = await _analyticsEngine.GetAggregatedMetricsAsync(entityId, request.MetricTypes, range, "average");
                }
                else
                {
                    // Weighbridge comparison - would need organization context
                    metrics = new MetricResultCollection
                    {
                        Metrics = new List<MetricResult>(),
                        GeneratedAt = DateTime.UtcNow,
                        TimeRange = request.TimeRange
                    };
                }
                
                results[entityId] = metrics;
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

public class UpdateAnomalyStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public string? ResolvedBy { get; set; }
}

public class CompareMetricsRequest
{
    public List<string> EntityIds { get; set; } = new();
    public List<string> MetricTypes { get; set; } = new();
    public string TimeRange { get; set; } = "30d";
    public string ComparisonType { get; set; } = "organization"; // organization or weighbridge
}