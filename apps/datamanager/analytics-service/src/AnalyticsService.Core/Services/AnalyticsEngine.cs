using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Interfaces;
using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Services;

public class AnalyticsEngine : IAnalyticsEngine
{
    private readonly IAnalyticsMetricRepository _metricRepository;
    private readonly IOperationalMetricsCalculator _operationalCalculator;
    private readonly IFinancialMetricsCalculator _financialCalculator;
    private readonly IComplianceMetricsCalculator _complianceCalculator;
    private readonly ITrendAnalysisService _trendAnalysisService;
    private readonly IAnomalyDetectionService _anomalyDetectionService;
    private readonly IForecastingService _forecastingService;
    private readonly IBenchmarkService _benchmarkService;

    public AnalyticsEngine(
        IAnalyticsMetricRepository metricRepository,
        IOperationalMetricsCalculator operationalCalculator,
        IFinancialMetricsCalculator financialCalculator,
        IComplianceMetricsCalculator complianceCalculator,
        ITrendAnalysisService trendAnalysisService,
        IAnomalyDetectionService anomalyDetectionService,
        IForecastingService forecastingService,
        IBenchmarkService benchmarkService)
    {
        _metricRepository = metricRepository;
        _operationalCalculator = operationalCalculator;
        _financialCalculator = financialCalculator;
        _complianceCalculator = complianceCalculator;
        _trendAnalysisService = trendAnalysisService;
        _anomalyDetectionService = anomalyDetectionService;
        _forecastingService = forecastingService;
        _benchmarkService = benchmarkService;
    }

    public async Task<MetricResult> CalculateTransactionVolumeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null)
    {
        return await _operationalCalculator.CalculateTransactionVolumeAsync(organizationId, timeRange, weighbridgeId);
    }

    public async Task<MetricResult> CalculateWeighbridgeUtilizationAsync(string weighbridgeId, TimeRange timeRange)
    {
        return await _operationalCalculator.CalculateWeighbridgeUtilizationAsync(weighbridgeId, timeRange);
    }

    public async Task<MetricResult> CalculateAverageProcessingTimeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null)
    {
        return await _operationalCalculator.CalculateProcessingTimeAsync(organizationId, timeRange, weighbridgeId);
    }

    public async Task<MetricResult> CalculateRevenueTotalAsync(string organizationId, TimeRange timeRange)
    {
        return await _financialCalculator.CalculateTotalRevenueAsync(organizationId, timeRange);
    }

    public async Task<MetricResult> CalculateComplianceRateAsync(string organizationId, TimeRange timeRange)
    {
        return await _complianceCalculator.CalculateComplianceRateAsync(organizationId, timeRange);
    }

    public async Task<TimeSeriesResult> GetMetricTimeSeriesAsync(string organizationId, string metricType, TimeRange timeRange, string timeGranularity, string? weighbridgeId = null)
    {
        var metrics = await _metricRepository.GetTimeSeriesAsync(organizationId, metricType, timeRange.From, timeRange.To, weighbridgeId);
        
        var dataPoints = metrics.Select(m => new TimeSeriesPoint
        {
            Timestamp = m.Timestamp,
            Value = m.Value,
            Metadata = m.Metadata
        }).ToList();

        return new TimeSeriesResult
        {
            MetricType = metricType,
            DataPoints = dataPoints,
            TimeGranularity = timeGranularity,
            PeriodStart = timeRange.From,
            PeriodEnd = timeRange.To
        };
    }

    public async Task<TrendAnalysisResponse> AnalyzeTrendsAsync(string organizationId, string metricType, TimeRange timeRange, string? weighbridgeId = null)
    {
        return await _trendAnalysisService.AnalyzeTrendAsync(organizationId, metricType, timeRange, weighbridgeId);
    }

    public async Task<List<AnomalyResponse>> DetectAnomaliesAsync(string organizationId, string metricType, TimeRange timeRange)
    {
        var anomalies = await _anomalyDetectionService.DetectAnomaliesAsync(organizationId, metricType, timeRange);
        
        return anomalies.Select(a => new AnomalyResponse
        {
            Id = a.Id,
            MetricType = a.MetricType,
            Timestamp = a.Timestamp,
            ActualValue = a.ActualValue,
            ExpectedValue = a.ExpectedValue,
            Deviation = a.Deviation,
            DeviationPercentage = a.DeviationPercentage,
            Severity = a.Severity,
            AnomalyType = a.AnomalyType,
            ConfidenceScore = a.ConfidenceScore,
            Status = a.Status,
            Context = a.Context,
            Resolution = a.Resolution
        }).ToList();
    }

    public async Task<ForecastResult> GenerateForecastAsync(string organizationId, string metricType, int forecastDays, string? weighbridgeId = null)
    {
        return await _forecastingService.GenerateLinearTrendForecastAsync(organizationId, metricType, forecastDays, weighbridgeId);
    }

    public async Task<List<BenchmarkComparison>> GetBenchmarkComparisonsAsync(string organizationId, List<string> metricTypes)
    {
        return await _benchmarkService.CompareToBenchmarksAsync(organizationId, metricTypes);
    }

    public async Task<MetricResultCollection> GetRealTimeMetricsAsync(string organizationId, List<string>? metricTypes = null)
    {
        var metrics = new List<MetricResult>();
        var timeRange = TimeRange.Last24Hours;

        // Default metric types if not specified
        metricTypes ??= new List<string> 
        { 
            "TransactionVolume", 
            "AverageProcessingTime", 
            "WeighbridgeUtilization", 
            "Revenue", 
            "ComplianceRate" 
        };

        foreach (var metricType in metricTypes)
        {
            try
            {
                MetricResult metric = metricType switch
                {
                    "TransactionVolume" => await CalculateTransactionVolumeAsync(organizationId, timeRange),
                    "AverageProcessingTime" => await CalculateAverageProcessingTimeAsync(organizationId, timeRange),
                    "Revenue" => await CalculateRevenueTotalAsync(organizationId, timeRange),
                    "ComplianceRate" => await CalculateComplianceRateAsync(organizationId, timeRange),
                    _ => await GetLatestMetricAsync(organizationId, metricType)
                };

                metrics.Add(metric);
            }
            catch (Exception ex)
            {
                // Log exception and continue with other metrics
                // For now, add a placeholder metric
                metrics.Add(new MetricResult
                {
                    MetricType = metricType,
                    Name = metricType,
                    Value = 0,
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { ["error"] = ex.Message }
                });
            }
        }

        return new MetricResultCollection
        {
            Metrics = metrics,
            GeneratedAt = DateTime.UtcNow,
            TimeRange = "24h",
            Summary = new Dictionary<string, object>
            {
                ["totalMetrics"] = metrics.Count,
                ["organizationId"] = organizationId
            }
        };
    }

    public async Task<MetricResultCollection> GetAggregatedMetricsAsync(string organizationId, List<string> metricTypes, TimeRange timeRange, string aggregationType)
    {
        var metrics = new List<MetricResult>();

        foreach (var metricType in metricTypes)
        {
            var timeSeriesData = await _metricRepository.GetTimeSeriesAsync(organizationId, metricType, timeRange.From, timeRange.To);
            var values = timeSeriesData.Select(m => m.Value);

            double aggregatedValue = aggregationType.ToLower() switch
            {
                "sum" => values.Sum(),
                "average" => values.Any() ? values.Average() : 0,
                "min" => values.Any() ? values.Min() : 0,
                "max" => values.Any() ? values.Max() : 0,
                "count" => values.Count(),
                _ => values.Any() ? values.Average() : 0
            };

            metrics.Add(new MetricResult
            {
                MetricType = metricType,
                Name = $"{metricType} ({aggregationType})",
                Value = aggregatedValue,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["aggregationType"] = aggregationType,
                    ["dataPoints"] = values.Count(),
                    ["timeRange"] = $"{timeRange.From:yyyy-MM-dd} to {timeRange.To:yyyy-MM-dd}"
                }
            });
        }

        return new MetricResultCollection
        {
            Metrics = metrics,
            GeneratedAt = DateTime.UtcNow,
            TimeRange = $"{timeRange.From:yyyy-MM-dd} to {timeRange.To:yyyy-MM-dd}",
            Summary = new Dictionary<string, object>
            {
                ["aggregationType"] = aggregationType,
                ["totalMetrics"] = metrics.Count
            }
        };
    }

    private async Task<MetricResult> GetLatestMetricAsync(string organizationId, string metricType)
    {
        var latestMetric = await _metricRepository.GetLatestMetricAsync(organizationId, metricType);
        
        if (latestMetric == null)
        {
            return new MetricResult
            {
                MetricType = metricType,
                Name = metricType,
                Value = 0,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["status"] = "no_data" }
            };
        }

        return new MetricResult
        {
            MetricType = latestMetric.MetricType,
            Name = latestMetric.Name,
            Value = latestMetric.Value,
            Unit = latestMetric.Unit,
            Timestamp = latestMetric.Timestamp,
            PreviousValue = latestMetric.PreviousValue,
            Change = latestMetric.Value - (latestMetric.PreviousValue ?? 0),
            PercentageChange = latestMetric.PercentageChange,
            Trend = latestMetric.TrendDirection,
            Breakdown = latestMetric.Breakdown,
            Metadata = latestMetric.Metadata
        };
    }
}