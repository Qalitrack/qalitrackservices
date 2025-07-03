using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.DTOs;

public class ForecastResult
{
    public string MetricType { get; set; } = string.Empty;
    public int ForecastPeriod { get; set; }
    public List<ForecastPoint> Predictions { get; set; } = new();
    public ConfidenceInterval ConfidenceInterval { get; set; } = new();
    public double Accuracy { get; set; }
    public string Model { get; set; } = string.Empty;
    public Dictionary<string, object> ModelParameters { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ForecastPoint
{
    public DateTime Timestamp { get; set; }
    public double PredictedValue { get; set; }
    public double? LowerBound { get; set; }
    public double? UpperBound { get; set; }
    public double Confidence { get; set; }
}

public class ConfidenceInterval
{
    public double Level { get; set; } = 0.95;
    public List<double> LowerBounds { get; set; } = new();
    public List<double> UpperBounds { get; set; } = new();
}

public class AnomalyResponse
{
    public Guid Id { get; set; }
    public string MetricType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double ActualValue { get; set; }
    public double ExpectedValue { get; set; }
    public double Deviation { get; set; }
    public double DeviationPercentage { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string AnomalyType { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
    public string? Resolution { get; set; }
}

public class TrendAnalysisResponse
{
    public string MetricType { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty;
    public double TrendStrength { get; set; }
    public double Slope { get; set; }
    public double RSquared { get; set; }
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    public List<TrendBreakpoint> Breakpoints { get; set; } = new();
    public Dictionary<string, object> Insights { get; set; } = new();
    public double ConfidenceLevel { get; set; }
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
}

public class BenchmarkComparison
{
    public string MetricType { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double BenchmarkValue { get; set; }
    public double Difference { get; set; }
    public double PercentageDifference { get; set; }
    public string Performance { get; set; } = string.Empty; // Above, Below, AtBenchmark
    public string BenchmarkType { get; set; } = string.Empty;
    public string BenchmarkSource { get; set; } = string.Empty;
    public double? Percentile { get; set; }
    public Dictionary<string, double> PercentileRanges { get; set; } = new();
}