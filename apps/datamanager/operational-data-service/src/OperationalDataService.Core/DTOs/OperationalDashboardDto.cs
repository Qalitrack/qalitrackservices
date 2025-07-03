using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class OperationalDashboardDto
{
    public string OrganizationId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public WeighbridgeStatusSummary WeighbridgeStatus { get; set; } = new();
    public CapacityUtilizationSummary CapacityUtilization { get; set; } = new();
    public ProductSummary ActiveProducts { get; set; } = new();
    public RoutePerformanceSummary RoutePerformance { get; set; } = new();
    public List<OperationalAlertSummary> OperationalAlerts { get; set; } = new();
    public MaintenanceScheduleSummary MaintenanceSchedule { get; set; } = new();
    public PerformanceMetricsSummary PerformanceMetrics { get; set; } = new();
    public List<CapacityForecastSummary> Forecasts { get; set; } = new();
    public SystemHealthSummary SystemHealth { get; set; } = new();
}

public class WeighbridgeStatusSummary
{
    public int TotalWeighbridges { get; set; }
    public int ActiveWeighbridges { get; set; }
    public int MaintenanceWeighbridges { get; set; }
    public int OfflineWeighbridges { get; set; }
    public decimal AverageUtilization { get; set; }
    public int TotalVehiclesInQueue { get; set; }
    public TimeSpan AverageWaitTime { get; set; }
    public List<WeighbridgeOperationalStatus> WeighbridgeDetails { get; set; } = new();
}

public class WeighbridgeOperationalStatus
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public OperationalStatus Status { get; set; }
    public decimal UtilizationRate { get; set; }
    public int VehiclesInQueue { get; set; }
    public TimeSpan EstimatedWaitTime { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CapacityUtilizationSummary
{
    public decimal OverallUtilization { get; set; }
    public decimal PeakUtilization { get; set; }
    public DateTime PeakTime { get; set; }
    public decimal LowestUtilization { get; set; }
    public DateTime LowestTime { get; set; }
    public List<HourlyUtilization> HourlyBreakdown { get; set; } = new();
    public List<CapacityAlert> ActiveAlerts { get; set; } = new();
    public UtilizationTrend Trend { get; set; }
}

public class HourlyUtilization
{
    public DateTime Hour { get; set; }
    public decimal UtilizationRate { get; set; }
    public int VehicleCount { get; set; }
    public TimeSpan AverageWaitTime { get; set; }
}

public class ProductSummary
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public DateTime LastSyncTime { get; set; }
    public List<ProductAlert> ProductAlerts { get; set; } = new();
    public List<TopProduct> TopProducts { get; set; } = new();
}

public class ProductAlert
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TopProduct
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal QuantityMoved { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}

public class RoutePerformanceSummary
{
    public int TotalRoutes { get; set; }
    public int ActiveRoutes { get; set; }
    public decimal AverageReliabilityScore { get; set; }
    public decimal AverageDelay { get; set; }
    public List<RoutePerformanceDetail> TopPerformingRoutes { get; set; } = new();
    public List<RoutePerformanceDetail> PoorPerformingRoutes { get; set; } = new();
    public List<RouteIssue> ActiveIssues { get; set; } = new();
}

public class RoutePerformanceDetail
{
    public string RouteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ReliabilityScore { get; set; }
    public decimal AverageDelay { get; set; }
    public int UsageCount { get; set; }
    public decimal CustomerSatisfaction { get; set; }
}

public class OperationalAlertSummary
{
    public string AlertId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public AlertStatus Status { get; set; }
    public DateTime TriggeredAt { get; set; }
    public string? WeighbridgeId { get; set; }
    public string? WeighbridgeName { get; set; }
}

public class MaintenanceScheduleSummary
{
    public int TotalScheduledMaintenance { get; set; }
    public int OverdueMaintenance { get; set; }
    public int TodaysMaintenance { get; set; }
    public int ThisWeeksMaintenance { get; set; }
    public List<UpcomingMaintenance> UpcomingMaintenance { get; set; } = new();
    public List<OverdueMaintenance> OverdueMaintenance { get; set; } = new();
}

public class UpcomingMaintenance
{
    public string MaintenanceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string WeighbridgeName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenancePriority Priority { get; set; }
}

public class OverdueMaintenance
{
    public string MaintenanceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string WeighbridgeName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public int DaysOverdue { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenancePriority Priority { get; set; }
}

public class PerformanceMetricsSummary
{
    public decimal OverallEfficiency { get; set; }
    public decimal ThroughputRate { get; set; }
    public decimal QualityScore { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }
    public decimal CostEfficiency { get; set; }
    public List<KpiMetric> KeyMetrics { get; set; } = new();
    public List<PerformanceTrend> Trends { get; set; } = new();
}

public class KpiMetric
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Target { get; set; }
    public string Unit { get; set; } = string.Empty;
    public MetricStatus Status { get; set; }
    public decimal PercentageOfTarget { get; set; }
    public TrendDirection Trend { get; set; }
}

public class CapacityForecastSummary
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string WeighbridgeName { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; }
    public decimal PredictedPeakUtilization { get; set; }
    public DateTime PredictedPeakTime { get; set; }
    public ForecastConfidence Confidence { get; set; }
    public List<string> KeyFactors { get; set; } = new();
}

public class SystemHealthSummary
{
    public HealthStatus OverallHealth { get; set; }
    public decimal SystemUptime { get; set; }
    public int ActiveConnections { get; set; }
    public decimal ResponseTime { get; set; }
    public decimal ErrorRate { get; set; }
    public List<SystemComponent> Components { get; set; } = new();
    public DateTime LastHealthCheck { get; set; }
}

public class SystemComponent
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public decimal ResponseTime { get; set; }
    public decimal ErrorRate { get; set; }
    public DateTime LastChecked { get; set; }
    public string? Message { get; set; }
}

public enum UtilizationTrend
{
    Increasing,
    Stable,
    Decreasing,
    Volatile
}

public enum MetricStatus
{
    OnTarget,
    AboveTarget,
    BelowTarget,
    Critical
}

public enum HealthStatus
{
    Healthy,
    Warning,
    Critical,
    Offline
}