namespace TransporterService.Core.Entities;

public enum PerformanceMetricType
{
    OnTimeDelivery,
    SafetyRecord,
    FuelEfficiency,
    CustomerSatisfaction,
    CostEffectiveness,
    VehicleUtilization,
    MaintenanceRecord
}

public class TransporterPerformance : BaseEntity
{
    public string TransporterId { get; set; } = string.Empty;
    public PerformanceMetricType MetricType { get; set; }
    public DateTime RecordDate { get; set; }
    public string Period { get; set; } = string.Empty; // e.g., "2024-Q1", "2024-01"
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty; // e.g., "%", "km/L", "score"
    public decimal? TargetValue { get; set; }
    public decimal? BenchmarkValue { get; set; }
    public string? Grade { get; set; } // A, B, C, D, F
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
    
    // Navigation properties
    public virtual Transporter Transporter { get; set; } = null!;
}