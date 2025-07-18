using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class MonitoringDto
{
    public string Id { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string? OperationalId { get; set; }
    public string? ProcessId { get; set; }
    public MonitoringType MonitoringType { get; set; }
    public MetricType MetricType { get; set; }
    public decimal MetricValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public AlertLevel AlertLevel { get; set; }
    public bool IsAlert { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AlertMessage { get; set; }
    public Dictionary<string, object>? MetricData { get; set; }
    public Dictionary<string, object>? ContextData { get; set; }
}

public class RecordMetricRequest
{
    public string MetricName { get; set; } = string.Empty;
    public string? OperationalId { get; set; }
    public string? ProcessId { get; set; }
    public MonitoringType MonitoringType { get; set; }
    public MetricType MetricType { get; set; }
    public decimal MetricValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? MetricData { get; set; }
    public Dictionary<string, object>? ContextData { get; set; }
}

public class UpdateThresholdsRequest
{
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
}

public class ThresholdSettingsDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class SystemHealthDto
{
    public string OverallStatus { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public int TotalActiveOperations { get; set; }
    public int TotalActiveProcesses { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public decimal CpuUsage { get; set; }
    public decimal MemoryUsage { get; set; }
    public List<MonitoringDto> RecentMetrics { get; set; } = new List<MonitoringDto>();
}

public class OperationHealthDto
{
    public string OperationId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public int ActiveProcesses { get; set; }
    public int Alerts { get; set; }
    public List<MonitoringDto> RecentMetrics { get; set; } = new List<MonitoringDto>();
}

public class ProcessHealthDto
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public int Alerts { get; set; }
    public List<MonitoringDto> RecentMetrics { get; set; } = new List<MonitoringDto>();
}

public class MetricStatisticsDto
{
    public string MetricName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int SampleCount { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal AverageValue { get; set; }
    public decimal MedianValue { get; set; }
    public decimal StandardDeviation { get; set; }
}

public class TrendAnalysisDto
{
    public string MetricName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal? MovingAverage { get; set; }
    public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
}

public class PerformanceReportDto
{
    public string OperationId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<MetricStatisticsDto> MetricStatistics { get; set; } = new List<MetricStatisticsDto>();
    public List<TrendAnalysisDto> TrendAnalysis { get; set; } = new List<TrendAnalysisDto>();
}

public class BusinessMetricsDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalOperations { get; set; }
    public int CompletedOperations { get; set; }
    public int FailedOperations { get; set; }
    public decimal SuccessRate { get; set; }
    public TimeSpan AverageOperationDuration { get; set; }
}

public class TransactionMetricsDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal TransactionSuccessRate { get; set; }
    public decimal AverageTransactionTime { get; set; }
}

public class ErrorMetricsDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalErrors { get; set; }
    public int CriticalErrors { get; set; }
    public int WarningErrors { get; set; }
    public decimal ErrorRate { get; set; }
    public List<string> TopErrorMessages { get; set; } = new List<string>();
}

public class RegisterCustomMetricRequest
{
    public string MetricName { get; set; } = string.Empty;
    public MonitoringType MonitoringType { get; set; }
    public MetricType MetricType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
}

public class CustomMetricDefinitionDto
{
    public string MetricName { get; set; } = string.Empty;
    public MonitoringType MonitoringType { get; set; }
    public MetricType MetricType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ThresholdSettingsDto? Thresholds { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class UpdateCustomMetricDefinitionRequest
{
    public string? Description { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
}