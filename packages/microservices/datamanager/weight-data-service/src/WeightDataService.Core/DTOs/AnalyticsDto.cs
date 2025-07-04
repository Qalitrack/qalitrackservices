namespace WeightDataService.Core.DTOs;

public class AnalyticsSummaryDto
{
    public int TotalMeasurements { get; set; }
    public int PendingMeasurements { get; set; }
    public int ValidatedMeasurements { get; set; }
    public int CorrectedMeasurements { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal AverageWeight { get; set; }
    public int ActiveWeighbridges { get; set; }
    public int MaintenanceWeighbridges { get; set; }
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public List<WeighbridgeUsageDto> WeighbridgeUsage { get; set; } = new();
    public List<DailyWeightDto> DailyWeights { get; set; } = new();
}

public class WeighbridgeUsageDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MeasurementCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal AverageWeight { get; set; }
}

public class DailyWeightDto
{
    public DateTime Date { get; set; }
    public int MeasurementCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal AverageWeight { get; set; }
}

public class AnalyticsRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? WeighbridgeId { get; set; }
    public bool IncludeWeighbridgeUsage { get; set; } = true;
    public bool IncludeDailyBreakdown { get; set; } = true;
}