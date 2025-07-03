using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class WeighbridgeCapacity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CurrentLoad { get; set; }
    public int MaxCapacity { get; set; }
    public int HourlyCapacity { get; set; }
    public decimal UtilizationRate { get; set; }
    public int VehiclesInQueue { get; set; }
    public TimeSpan EstimatedWaitTime { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public DateTime? NextAvailableSlot { get; set; }
    public OperationalStatus Status { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class WeighbridgeAvailability
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime RequestedTime { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }
    public TimeSpan? EstimatedWaitTime { get; set; }
    public decimal UtilizationRate { get; set; }
    public List<string> Restrictions { get; set; } = new();
    public AvailabilityReason Reason { get; set; }
    public string? Notes { get; set; }
}

public class CapacityForecast
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; }
    public int ForecastHours { get; set; }
    public List<HourlyCapacityForecast> HourlyForecasts { get; set; } = new();
    public CapacityTrend Trend { get; set; }
    public ForecastConfidence Confidence { get; set; }
    public List<string> Assumptions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class HourlyCapacityForecast
{
    public DateTime Hour { get; set; }
    public int PredictedLoad { get; set; }
    public decimal PredictedUtilization { get; set; }
    public TimeSpan PredictedWaitTime { get; set; }
    public ForecastConfidence Confidence { get; set; }
    public List<string> Factors { get; set; } = new();
}

public class LoadBalancingResult
{
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public List<WeighbridgeAssignment> Assignments { get; set; } = new();
    public List<LoadBalancingMetric> Metrics { get; set; } = new();
    public LoadBalancingStrategy Strategy { get; set; }
    public decimal ImprovementScore { get; set; }
    public string? Recommendations { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class WeighbridgeAssignment
{
    public string TransactionId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime EstimatedStartTime { get; set; }
    public DateTime EstimatedEndTime { get; set; }
    public AssignmentPriority Priority { get; set; }
    public string? Reason { get; set; }
    public decimal Score { get; set; }
    public List<string> Constraints { get; set; } = new();
}

public class LoadBalancingMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime MeasuredAt { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public MetricType Type { get; set; }
}

public class CapacityAnalytics
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public TimeRange Period { get; set; } = new();
    public decimal AverageUtilization { get; set; }
    public decimal PeakUtilization { get; set; }
    public DateTime PeakTime { get; set; }
    public decimal LowestUtilization { get; set; }
    public DateTime LowestTime { get; set; }
    public List<CapacityPattern> Patterns { get; set; } = new();
    public List<CapacityBottleneck> Bottlenecks { get; set; } = new();
    public List<CapacityRecommendation> Recommendations { get; set; } = new();
}

public class CapacityPattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PatternType Type { get; set; }
    public List<TimeSpan> RecurringTimes { get; set; } = new();
    public decimal AverageImpact { get; set; }
    public decimal Confidence { get; set; }
}

public class CapacityBottleneck
{
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BottleneckType Type { get; set; }
    public decimal ImpactScore { get; set; }
    public DateTime FirstObserved { get; set; }
    public DateTime LastObserved { get; set; }
    public int Frequency { get; set; }
    public List<string> Causes { get; set; } = new();
    public List<string> PotentialSolutions { get; set; } = new();
}

public class CapacityRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationType Type { get; set; }
    public RecommendationPriority Priority { get; set; }
    public decimal EstimatedImpact { get; set; }
    public decimal? EstimatedCost { get; set; }
    public TimeSpan? ImplementationTime { get; set; }
    public List<string> Benefits { get; set; } = new();
    public List<string> Risks { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum AvailabilityReason
{
    Available,
    AtCapacity,
    Maintenance,
    Offline,
    Restricted,
    OutOfService
}

public enum CapacityTrend
{
    Increasing,
    Stable,
    Decreasing,
    Volatile
}

public enum LoadBalancingStrategy
{
    EvenDistribution,
    CapacityBased,
    PerformanceBased,
    PriorityBased,
    TimeOptimized,
    CostOptimized
}

public enum AssignmentPriority
{
    Low,
    Normal,
    High,
    Critical,
    Emergency
}

public enum MetricType
{
    Utilization,
    Throughput,
    WaitTime,
    Efficiency,
    Quality,
    Cost
}

public enum PatternType
{
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Seasonal,
    Custom
}

public enum BottleneckType
{
    Capacity,
    Process,
    Resource,
    System,
    External
}

public enum RecommendationType
{
    Capacity,
    Process,
    Technology,
    Staffing,
    Scheduling,
    Infrastructure
}