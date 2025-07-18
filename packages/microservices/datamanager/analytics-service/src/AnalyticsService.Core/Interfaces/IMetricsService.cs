using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Interfaces;

public interface IMetricsService
{
    // Metrics Management
    Task<MetricsDto> CreateMetricAsync(CreateMetricsRequest request);
    Task<MetricsDto?> GetMetricAsync(string metricId);
    Task<MetricsDto> UpdateMetricAsync(string metricId, UpdateMetricsRequest request);
    Task<bool> DeleteMetricAsync(string metricId);
    Task<List<MetricsDto>> GetMetricsByOrganizationAsync(string organizationId);
    Task<List<MetricsDto>> GetMetricsByTypeAsync(MetricType metricType);
    Task<List<MetricsDto>> GetMetricsByCategoryAsync(MetricCategory category);
    
    // KPI Management
    Task<List<MetricsDto>> GetKPIsAsync(string organizationId);
    Task<KPIDashboardDto> GetKPIDashboardAsync(string organizationId);
    Task<bool> SetKPITargetAsync(string metricId, decimal targetValue);
    Task<bool> UpdateKPIThresholdsAsync(string metricId, UpdateThresholdsRequest request);
    
    // Metrics Calculation
    Task<MetricCalculationResultDto> CalculateMetricAsync(string metricId);
    Task<List<MetricCalculationResultDto>> CalculateMetricsBatchAsync(List<string> metricIds);
    Task<MetricCalculationResultDto> RecalculateMetricAsync(string metricId);
    Task<bool> ScheduleMetricCalculationAsync(string metricId, ScheduleRequest schedule);
    
    // Real-time Metrics
    Task<bool> EnableRealTimeMetricAsync(string metricId);
    Task<bool> DisableRealTimeMetricAsync(string metricId);
    Task<List<MetricsDto>> GetRealTimeMetricsAsync(string organizationId);
    Task<MetricValueDto> GetCurrentMetricValueAsync(string metricId);
    
    // Trend Analysis
    Task<MetricTrendDto> GetMetricTrendAsync(string metricId, DateTime fromDate, DateTime toDate);
    Task<List<MetricTrendDto>> GetTrendAnalysisAsync(List<string> metricIds, DateTime fromDate, DateTime toDate);
    Task<MetricForecastDto> GetMetricForecastAsync(string metricId, int forecastDays);
    
    // Benchmarking
    Task<MetricBenchmarkDto> GetMetricBenchmarkAsync(string metricId, string? industry = null);
    Task<List<MetricBenchmarkDto>> GetBenchmarkAnalysisAsync(string organizationId, string? industry = null);
    Task<bool> UpdateBenchmarkDataAsync(string metricId, List<BenchmarkDataPoint> benchmarkData);
    
    // Alerting
    Task<bool> EnableMetricAlertingAsync(string metricId, AlertConfiguration alertConfig);
    Task<bool> DisableMetricAlertingAsync(string metricId);
    Task<List<MetricAlertDto>> GetActiveAlertsAsync(string organizationId);
    Task<bool> AcknowledgeAlertAsync(string alertId, string userId);
    
    // Historical Data
    Task<List<HistoricalMetricValue>> GetMetricHistoryAsync(string metricId, DateTime fromDate, DateTime toDate);
    Task<MetricStatisticsDto> GetMetricStatisticsAsync(string metricId, DateTime fromDate, DateTime toDate);
    Task<bool> ArchiveMetricDataAsync(string metricId, DateTime beforeDate);
    
    // Metric Dependencies
    Task<List<MetricsDto>> GetDependentMetricsAsync(string metricId);
    Task<List<MetricsDto>> GetMetricDependenciesAsync(string metricId);
    Task<bool> ValidateMetricDependenciesAsync(string metricId);
    Task<MetricImpactAnalysisDto> AnalyzeMetricImpactAsync(string metricId);
}