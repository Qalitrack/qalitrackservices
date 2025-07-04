using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Interfaces;

public interface IAnalyticsEngine
{
    // Core Metrics Calculation
    Task<MetricResult> CalculateTransactionVolumeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null);
    Task<MetricResult> CalculateWeighbridgeUtilizationAsync(string weighbridgeId, TimeRange timeRange);
    Task<MetricResult> CalculateAverageProcessingTimeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null);
    Task<MetricResult> CalculateRevenueTotalAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateComplianceRateAsync(string organizationId, TimeRange timeRange);
    
    // Time Series Analytics
    Task<TimeSeriesResult> GetMetricTimeSeriesAsync(string organizationId, string metricType, TimeRange timeRange, string timeGranularity, string? weighbridgeId = null);
    
    // Trend Analysis
    Task<TrendAnalysisResponse> AnalyzeTrendsAsync(string organizationId, string metricType, TimeRange timeRange, string? weighbridgeId = null);
    
    // Anomaly Detection
    Task<List<AnomalyResponse>> DetectAnomaliesAsync(string organizationId, string metricType, TimeRange timeRange);
    
    // Forecasting
    Task<ForecastResult> GenerateForecastAsync(string organizationId, string metricType, int forecastDays, string? weighbridgeId = null);
    
    // Benchmark Comparisons
    Task<List<BenchmarkComparison>> GetBenchmarkComparisonsAsync(string organizationId, List<string> metricTypes);
    
    // Real-time Metrics
    Task<MetricResultCollection> GetRealTimeMetricsAsync(string organizationId, List<string>? metricTypes = null);
    
    // Aggregated Metrics
    Task<MetricResultCollection> GetAggregatedMetricsAsync(string organizationId, List<string> metricTypes, TimeRange timeRange, string aggregationType);
}

public class TimeRange
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public TimeSpan Duration => To - From;
    
    public static TimeRange Last24Hours => new() { From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow };
    public static TimeRange Last7Days => new() { From = DateTime.UtcNow.AddDays(-7), To = DateTime.UtcNow };
    public static TimeRange Last30Days => new() { From = DateTime.UtcNow.AddDays(-30), To = DateTime.UtcNow };
    public static TimeRange Last90Days => new() { From = DateTime.UtcNow.AddDays(-90), To = DateTime.UtcNow };
    
    public static TimeRange FromString(string timeRange)
    {
        return timeRange.ToLower() switch
        {
            "1h" => new() { From = DateTime.UtcNow.AddHours(-1), To = DateTime.UtcNow },
            "24h" => Last24Hours,
            "7d" => Last7Days,
            "30d" => Last30Days,
            "90d" => Last90Days,
            _ => Last24Hours
        };
    }
}