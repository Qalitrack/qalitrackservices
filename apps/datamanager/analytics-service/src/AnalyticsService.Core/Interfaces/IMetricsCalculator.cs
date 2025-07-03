using AnalyticsService.Core.DTOs;

namespace AnalyticsService.Core.Interfaces;

public interface IMetricsCalculator
{
    Task<double> CalculateSum(IEnumerable<double> values);
    Task<double> CalculateAverage(IEnumerable<double> values);
    Task<double> CalculateMedian(IEnumerable<double> values);
    Task<double> CalculateStandardDeviation(IEnumerable<double> values);
    Task<double> CalculatePercentile(IEnumerable<double> values, double percentile);
    Task<double> CalculatePercentageChange(double currentValue, double previousValue);
    Task<string> DetermineTrend(IEnumerable<double> values);
    Task<Dictionary<string, double>> CalculatePercentiles(IEnumerable<double> values, double[] percentiles);
}

public interface IOperationalMetricsCalculator
{
    Task<MetricResult> CalculateTransactionVolumeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null);
    Task<MetricResult> CalculateProcessingTimeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null);
    Task<MetricResult> CalculateThroughputAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null);
    Task<MetricResult> CalculateQueueTimeAsync(string organizationId, TimeRange timeRange, string? weighbridgeId = null);
    Task<MetricResult> CalculateWeighbridgeUtilizationAsync(string weighbridgeId, TimeRange timeRange);
}

public interface IFinancialMetricsCalculator
{
    Task<MetricResult> CalculateTotalRevenueAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateRevenueByServiceAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateAverageTransactionValueAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateRevenueGrowthAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateProfitMarginsAsync(string organizationId, TimeRange timeRange);
    Task<Dictionary<string, MetricResult>> CalculateRevenueBreakdownAsync(string organizationId, TimeRange timeRange);
}

public interface IComplianceMetricsCalculator
{
    Task<MetricResult> CalculateComplianceRateAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateViolationRateAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateAuditScoreAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateRegulationAdherenceAsync(string organizationId, TimeRange timeRange, string regulationType);
    Task<Dictionary<string, MetricResult>> CalculateComplianceBreakdownAsync(string organizationId, TimeRange timeRange);
}

public interface IQualityMetricsCalculator
{
    Task<MetricResult> CalculateAccuracyRateAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculatePrecisionRateAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateMeasurementConsistencyAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateErrorRateAsync(string organizationId, TimeRange timeRange);
    Task<MetricResult> CalculateCalibrationComplianceAsync(string organizationId, TimeRange timeRange);
}