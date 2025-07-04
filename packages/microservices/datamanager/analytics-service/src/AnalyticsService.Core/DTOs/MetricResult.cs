namespace AnalyticsService.Core.DTOs;

public class MetricResult
{
    public string MetricType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? PreviousValue { get; set; }
    public double? Change { get; set; }
    public double? PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty;
    public Dictionary<string, object> Breakdown { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MetricResultCollection
{
    public List<MetricResult> Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string TimeRange { get; set; } = string.Empty;
    public Dictionary<string, object> Summary { get; set; } = new();
}

public class TimeSeriesPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TimeSeriesResult
{
    public string MetricType { get; set; } = string.Empty;
    public List<TimeSeriesPoint> DataPoints { get; set; } = new();
    public string TimeGranularity { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}