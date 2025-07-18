namespace WeightDataService.Core.Entities;

public class HistoricalAnalysis : BaseEntity
{
    public string AnalysisId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string? VehicleRegistration { get; set; }
    public string? ProductType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public AnalysisType Type { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal AverageWeight { get; set; }
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal StandardDeviation { get; set; }
    public decimal TrendSlope { get; set; }
    public decimal TrendR2 { get; set; }
    public int MeasurementCount { get; set; }
    public string TrendCategory { get; set; } = string.Empty;
    public string? AnomaliesDetected { get; set; } // JSON for anomaly details
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public string OrganizationId { get; set; } = string.Empty;
}

public enum AnalysisType
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly,
    Custom,
    RealTime
}