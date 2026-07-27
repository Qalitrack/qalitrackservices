namespace Transaction.Core.DTOs;

public class TransactionStatsDto
{
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int ActiveCount { get; set; }
    public int OtherCount { get; set; }
    public int TodayCount { get; set; }
    public int TodayCompletedCount { get; set; }
    public int ThisWeekCount { get; set; }
    public decimal TotalNetWeight { get; set; }
    public decimal TodayNetWeight { get; set; }
    public List<DailyCountDto> WeeklyTrend { get; set; } = new();
    public List<NameCountDto> TopVehicles { get; set; } = new();
    public List<NameCountDto> CommodityMix { get; set; } = new();
    public int StuckCount { get; set; }
    public int? OldestActiveAgeMinutes { get; set; }
}

public class DailyCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class NameCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
