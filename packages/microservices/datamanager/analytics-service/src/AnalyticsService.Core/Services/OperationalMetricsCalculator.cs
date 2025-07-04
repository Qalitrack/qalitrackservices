using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Interfaces;

namespace AnalyticsService.Core.Services;

public class OperationalMetricsCalculator : IOperationalMetricsCalculator
{
    private readonly IAnalyticsMetricRepository _metricRepository;
    private readonly IMetricsCalculator _calculator;

    public OperationalMetricsCalculator(
        IAnalyticsMetricRepository metricRepository,
        IMetricsCalculator calculator)
    {
        _metricRepository = metricRepository;
        _calculator = calculator;
    }

    public async Task<MetricResult> CalculateTransactionVolumeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null)
    {
        // Get current period metrics
        var currentMetrics = await _metricRepository.GetByMetricTypeAsync(organizationId, "TransactionVolume", timeRange.From, timeRange.To);
        
        if (!string.IsNullOrEmpty(weighbridgeId))
            currentMetrics = currentMetrics.Where(m => m.WeighbridgeId == weighbridgeId);

        var currentTotal = currentMetrics.Sum(m => m.Value);

        // Get previous period for comparison
        var previousPeriod = new TimeRange 
        { 
            From = timeRange.From.AddDays(-(timeRange.To - timeRange.From).Days), 
            To = timeRange.From 
        };
        
        var previousMetrics = await _metricRepository.GetByMetricTypeAsync(organizationId, "TransactionVolume", previousPeriod.From, previousPeriod.To);
        
        if (!string.IsNullOrEmpty(weighbridgeId))
            previousMetrics = previousMetrics.Where(m => m.WeighbridgeId == weighbridgeId);

        var previousTotal = previousMetrics.Sum(m => m.Value);
        var change = currentTotal - previousTotal;
        var percentageChange = await _calculator.CalculatePercentageChange(currentTotal, previousTotal);

        // Calculate breakdown by hour/day
        var hourlyBreakdown = currentMetrics
            .GroupBy(m => m.Timestamp.Hour)
            .ToDictionary(g => $"hour_{g.Key}", g => g.Sum(m => m.Value));

        var dailyBreakdown = currentMetrics
            .GroupBy(m => m.Timestamp.Date)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Sum(m => m.Value));

        return new MetricResult
        {
            MetricType = "TransactionVolume",
            Name = "Transaction Volume",
            Value = currentTotal,
            Unit = "transactions",
            Timestamp = DateTime.UtcNow,
            PreviousValue = previousTotal,
            Change = change,
            PercentageChange = percentageChange,
            Trend = change > 0 ? "Up" : change < 0 ? "Down" : "Stable",
            Breakdown = new Dictionary<string, object>
            {
                ["by_hour"] = hourlyBreakdown,
                ["by_day"] = dailyBreakdown,
                ["weighbridge_id"] = weighbridgeId ?? "all"
            },
            Metadata = new Dictionary<string, object>
            {
                ["period_days"] = (timeRange.To - timeRange.From).Days,
                ["data_points"] = currentMetrics.Count()
            }
        };
    }

    public async Task<MetricResult> CalculateProcessingTimeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null)
    {
        var metrics = await _metricRepository.GetByMetricTypeAsync(organizationId, "ProcessingTime", timeRange.From, timeRange.To);
        
        if (!string.IsNullOrEmpty(weighbridgeId))
            metrics = metrics.Where(m => m.WeighbridgeId == weighbridgeId);

        var values = metrics.Select(m => m.Value).ToList();
        
        if (!values.Any())
        {
            return new MetricResult
            {
                MetricType = "ProcessingTime",
                Name = "Average Processing Time",
                Value = 0,
                Unit = "minutes",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["status"] = "no_data" }
            };
        }

        var averageTime = await _calculator.CalculateAverage(values);
        var median = await _calculator.CalculateMedian(values);
        var stdDev = await _calculator.CalculateStandardDeviation(values);

        // Get previous period for comparison
        var previousPeriod = new TimeRange 
        { 
            From = timeRange.From.AddDays(-(timeRange.To - timeRange.From).Days), 
            To = timeRange.From 
        };
        
        var previousMetrics = await _metricRepository.GetByMetricTypeAsync(organizationId, "ProcessingTime", previousPeriod.From, previousPeriod.To);
        
        if (!string.IsNullOrEmpty(weighbridgeId))
            previousMetrics = previousMetrics.Where(m => m.WeighbridgeId == weighbridgeId);

        var previousValues = previousMetrics.Select(m => m.Value);
        var previousAverage = previousValues.Any() ? await _calculator.CalculateAverage(previousValues) : 0;
        var percentageChange = await _calculator.CalculatePercentageChange(averageTime, previousAverage);

        // Calculate percentiles
        var percentiles = await _calculator.CalculatePercentiles(values, new double[] { 50, 90, 95, 99 });

        return new MetricResult
        {
            MetricType = "ProcessingTime",
            Name = "Average Processing Time",
            Value = Math.Round(averageTime, 2),
            Unit = "minutes",
            Timestamp = DateTime.UtcNow,
            PreviousValue = Math.Round(previousAverage, 2),
            Change = Math.Round(averageTime - previousAverage, 2),
            PercentageChange = Math.Round(percentageChange, 2),
            Trend = averageTime < previousAverage ? "Up" : averageTime > previousAverage ? "Down" : "Stable", // Lower is better
            Breakdown = new Dictionary<string, object>
            {
                ["median"] = Math.Round(median, 2),
                ["standard_deviation"] = Math.Round(stdDev, 2),
                ["percentiles"] = percentiles,
                ["min"] = Math.Round(values.Min(), 2),
                ["max"] = Math.Round(values.Max(), 2)
            },
            Metadata = new Dictionary<string, object>
            {
                ["sample_size"] = values.Count,
                ["weighbridge_id"] = weighbridgeId ?? "all"
            }
        };
    }

    public async Task<MetricResult> CalculateThroughputAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null)
    {
        var transactionMetrics = await _metricRepository.GetByMetricTypeAsync(organizationId, "TransactionVolume", timeRange.From, timeRange.To);
        
        if (!string.IsNullOrEmpty(weighbridgeId))
            transactionMetrics = transactionMetrics.Where(m => m.WeighbridgeId == weighbridgeId);

        var totalTransactions = transactionMetrics.Sum(m => m.Value);
        var periodHours = (timeRange.To - timeRange.From).TotalHours;
        var throughput = periodHours > 0 ? totalTransactions / periodHours : 0;

        // Calculate hourly throughput breakdown
        var hourlyThroughput = transactionMetrics
            .GroupBy(m => m.Timestamp.Hour)
            .ToDictionary(g => $"hour_{g.Key}", g => g.Sum(m => m.Value));

        return new MetricResult
        {
            MetricType = "Throughput",
            Name = "Throughput",
            Value = Math.Round(throughput, 2),
            Unit = "transactions/hour",
            Timestamp = DateTime.UtcNow,
            Breakdown = new Dictionary<string, object>
            {
                ["by_hour"] = hourlyThroughput,
                ["total_transactions"] = totalTransactions,
                ["period_hours"] = Math.Round(periodHours, 2)
            },
            Metadata = new Dictionary<string, object>
            {
                ["weighbridge_id"] = weighbridgeId ?? "all"
            }
        };
    }

    public async Task<MetricResult> CalculateQueueTimeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null)
    {
        var metrics = await _metricRepository.GetByMetricTypeAsync(organizationId, "QueueTime", timeRange.From, timeRange.To);
        
        if (!string.IsNullOrEmpty(weighbridgeId))
            metrics = metrics.Where(m => m.WeighbridgeId == weighbridgeId);

        var values = metrics.Select(m => m.Value).ToList();
        
        if (!values.Any())
        {
            return new MetricResult
            {
                MetricType = "QueueTime",
                Name = "Average Queue Time",
                Value = 0,
                Unit = "minutes",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["status"] = "no_data" }
            };
        }

        var averageQueueTime = await _calculator.CalculateAverage(values);
        var maxQueueTime = values.Max();
        var percentiles = await _calculator.CalculatePercentiles(values, new double[] { 50, 90, 95 });

        return new MetricResult
        {
            MetricType = "QueueTime",
            Name = "Average Queue Time",
            Value = Math.Round(averageQueueTime, 2),
            Unit = "minutes",
            Timestamp = DateTime.UtcNow,
            Breakdown = new Dictionary<string, object>
            {
                ["max_queue_time"] = Math.Round(maxQueueTime, 2),
                ["percentiles"] = percentiles
            },
            Metadata = new Dictionary<string, object>
            {
                ["sample_size"] = values.Count,
                ["weighbridge_id"] = weighbridgeId ?? "all"
            }
        };
    }

    public async Task<MetricResult> CalculateWeighbridgeUtilizationAsync(string weighbridgeId, TimeRange timeRange)
    {
        var metrics = await _metricRepository.GetByWeighbridgeAsync(weighbridgeId, timeRange.From, timeRange.To);
        var activeTimeMetrics = metrics.Where(m => m.MetricType == "ActiveTime").ToList();
        
        var totalPossibleTime = (timeRange.To - timeRange.From).TotalMinutes;
        var totalActiveTime = activeTimeMetrics.Sum(m => m.Value);
        var utilizationRate = totalPossibleTime > 0 ? (totalActiveTime / totalPossibleTime) * 100 : 0;

        // Calculate peak hours
        var hourlyUtilization = activeTimeMetrics
            .GroupBy(m => m.Timestamp.Hour)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Value));

        var peakHours = hourlyUtilization
            .Where(h => h.Value > 0)
            .OrderByDescending(h => h.Value)
            .Take(3)
            .Select(h => h.Key)
            .ToList();

        return new MetricResult
        {
            MetricType = "WeighbridgeUtilization",
            Name = "Weighbridge Utilization",
            Value = Math.Round(utilizationRate, 2),
            Unit = "percentage",
            Timestamp = DateTime.UtcNow,
            Breakdown = new Dictionary<string, object>
            {
                ["active_time_minutes"] = Math.Round(totalActiveTime, 2),
                ["idle_time_minutes"] = Math.Round(totalPossibleTime - totalActiveTime, 2),
                ["peak_hours"] = peakHours,
                ["hourly_breakdown"] = hourlyUtilization
            },
            Metadata = new Dictionary<string, object>
            {
                ["weighbridge_id"] = weighbridgeId,
                ["period_hours"] = Math.Round((timeRange.To - timeRange.From).TotalHours, 2)
            }
        };
    }
}