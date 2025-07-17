namespace WeightDataService.Core.DTOs;

public class HistoricalAnalysisDto
{
    public Guid Id { get; set; }
    public string AnalysisId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string? VehicleRegistration { get; set; }
    public string? ProductType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal TotalWeight { get; set; }
    public decimal AverageWeight { get; set; }
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal StandardDeviation { get; set; }
    public decimal TrendSlope { get; set; }
    public decimal TrendR2 { get; set; }
    public int MeasurementCount { get; set; }
    public string TrendCategory { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
}

public class CreateHistoricalAnalysisDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string? VehicleRegistration { get; set; }
    public string? ProductType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class TrendAnalysisDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string? VehicleRegistration { get; set; }
    public string? ProductType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TrendSlope { get; set; }
    public decimal TrendR2 { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public string TrendStrength { get; set; } = string.Empty;
    public List<AnomalyDto> Anomalies { get; set; } = new();
}

public class AnomalyDto
{
    public DateTime Timestamp { get; set; }
    public decimal Weight { get; set; }
    public decimal ExpectedWeight { get; set; }
    public decimal Deviation { get; set; }
    public string AnomalyType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}