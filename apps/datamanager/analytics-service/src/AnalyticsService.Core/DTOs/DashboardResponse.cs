namespace AnalyticsService.Core.DTOs;

public class DashboardResponse
{
    public string OrganizationId { get; set; } = string.Empty;
    public List<WidgetData> Widgets { get; set; } = new();
    public Dictionary<string, object> GlobalFilters { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public int RefreshInterval { get; set; }
}

public class WidgetData
{
    public Guid WidgetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public object Data { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Status { get; set; } = "Active";
}

public class KpiWidgetData
{
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? PreviousValue { get; set; }
    public double? Change { get; set; }
    public double? PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Green, Yellow, Red
    public double? Target { get; set; }
}

public class ChartWidgetData
{
    public List<ChartSeries> Series { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string ChartType { get; set; } = string.Empty; // Line, Bar, Pie, Area
    public Dictionary<string, object> Options { get; set; } = new();
}

public class ChartSeries
{
    public string Name { get; set; } = string.Empty;
    public List<double> Data { get; set; } = new();
    public string Color { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class TableWidgetData
{
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public List<TableColumn> Columns { get; set; } = new();
    public int TotalRows { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
}

public class TableColumn
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool Sortable { get; set; } = true;
    public string Format { get; set; } = string.Empty;
}