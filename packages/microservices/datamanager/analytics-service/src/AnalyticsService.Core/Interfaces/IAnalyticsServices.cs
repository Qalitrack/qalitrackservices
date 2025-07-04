using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Interfaces;

public interface IAnomalyDetectionService
{
    Task<List<Anomaly>> DetectAnomaliesAsync(string organizationId, string metricType, TimeRange timeRange);
    Task<List<Anomaly>> DetectRealTimeAnomaliesAsync(string organizationId, double value, string metricType, DateTime timestamp);
    Task<bool> IsAnomalousAsync(double value, IEnumerable<double> historicalValues, double threshold = 2.5);
    Task<double> CalculateZScoreAsync(double value, IEnumerable<double> historicalValues);
    Task UpdateAnomalyStatusAsync(Guid anomalyId, string status, string? resolution = null, string? resolvedBy = null);
}

public interface IForecastingService
{
    Task<ForecastResult> GenerateLinearTrendForecastAsync(string organizationId, string metricType, int forecastDays, string? weighbridgeId = null);
    Task<ForecastResult> GenerateSeasonalForecastAsync(string organizationId, string metricType, int forecastDays, string? weighbridgeId = null);
    Task<ForecastResult> GenerateMovingAverageForecastAsync(string organizationId, string metricType, int forecastDays, int windowSize = 7);
    Task<double> CalculateHistoricalAccuracyAsync(string metricType, int days = 30);
}

public interface ITrendAnalysisService
{
    Task<TrendAnalysisResponse> AnalyzeTrendAsync(string organizationId, string metricType, TimeRange timeRange, string? weighbridgeId = null);
    Task<double> CalculateLinearRegressionSlopeAsync(IEnumerable<(DateTime timestamp, double value)> dataPoints);
    Task<double> CalculateRSquaredAsync(IEnumerable<(DateTime timestamp, double value)> dataPoints);
    Task<List<TrendBreakpoint>> DetectBreakpointsAsync(IEnumerable<(DateTime timestamp, double value)> dataPoints);
    Task<string> DetermineTrendDirectionAsync(double slope, double rSquared);
}

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardDataAsync(string organizationId, string? dashboardId = null);
    Task<WidgetData> GetWidgetDataAsync(Guid widgetId);
    Task<List<WidgetData>> GetWidgetsDataAsync(List<Guid> widgetIds);
    Task RefreshWidgetDataAsync(Guid widgetId);
    Task<DashboardWidget> CreateWidgetAsync(DashboardWidget widget);
    Task<DashboardWidget> UpdateWidgetAsync(DashboardWidget widget);
    Task DeleteWidgetAsync(Guid widgetId);
}

public interface IReportingService
{
    Task<byte[]> GenerateReportAsync(Guid reportDefinitionId, Dictionary<string, object>? parameters = null);
    Task<byte[]> GenerateCustomReportAsync(List<string> metricTypes, TimeRange timeRange, string outputFormat, Dictionary<string, object>? parameters = null);
    Task ScheduleReportAsync(Guid reportDefinitionId);
    Task<List<ReportDefinition>> GetDueReportsAsync();
    Task ProcessScheduledReportsAsync();
    Task SendReportAsync(byte[] reportData, List<string> recipients, string subject, string fileName);
}

public interface IBenchmarkService
{
    Task<List<BenchmarkComparison>> CompareToBenchmarksAsync(string organizationId, List<string> metricTypes);
    Task<BenchmarkComparison> CompareSingleMetricAsync(string organizationId, string metricType, string benchmarkType);
    Task<BenchmarkData> CreateBenchmarkAsync(BenchmarkData benchmark);
    Task<BenchmarkData> UpdateBenchmarkAsync(BenchmarkData benchmark);
    Task<List<BenchmarkData>> GetBenchmarksAsync(string metricType);
}

public interface IRealTimeMetricsService
{
    Task ProcessWeightMeasurementAsync(WeightMeasurement measurement);
    Task ProcessTransactionCompletedAsync(Transaction transaction);
    Task ProcessComplianceEventAsync(ComplianceEvent complianceEvent);
    Task BroadcastMetricUpdateAsync(string organizationId, MetricResult metric);
    Task<MetricResultCollection> GetCurrentMetricsAsync(string organizationId);
}

// External data models that would come from other services
public class WeightMeasurement
{
    public Guid Id { get; set; }
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public double Weight { get; set; }
    public DateTime Timestamp { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class Transaction
{
    public Guid Id { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TransactionCharge> Charges { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TransactionCharge
{
    public string ChargeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ComplianceEvent
{
    public Guid Id { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Compliant, Violation
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}