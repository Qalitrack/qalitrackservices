using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterPerformanceDto
{
    public string Id { get; set; } = string.Empty;
    public string TransporterId { get; set; } = string.Empty;
    public PerformanceMetricType MetricType { get; set; }
    public DateTime RecordDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? TargetValue { get; set; }
    public decimal? BenchmarkValue { get; set; }
    public string? Grade { get; set; }
    public string? Comments { get; set; }
    public int TotalTrips { get; set; }
    public int SuccessfulTrips { get; set; }
    public int DelayedTrips { get; set; }
    public int CancelledTrips { get; set; }
    public decimal TotalDistance { get; set; }
    public decimal TotalFuelConsumed { get; set; }
    public int AccidentCount { get; set; }
    public int ViolationCount { get; set; }
    public decimal CustomerRating { get; set; }
    public string? ImprovementAreas { get; set; }
    public string? Recommendations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TransporterPerformanceSummaryDto
{
    public string TransporterId { get; set; } = string.Empty;
    public string TransporterName { get; set; } = string.Empty;
    public decimal OverallRating { get; set; }
    public decimal OnTimeDeliveryRate { get; set; }
    public decimal SafetyScore { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }
    public int TotalTripsCompleted { get; set; }
    public decimal TotalDistanceCovered { get; set; }
    public int TotalAccidents { get; set; }
    public int TotalViolations { get; set; }
    public DateTime LastUpdated { get; set; }
}