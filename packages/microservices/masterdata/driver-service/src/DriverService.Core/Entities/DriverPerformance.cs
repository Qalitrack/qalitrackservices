namespace DriverService.Core.Entities;

public enum PerformancePeriod
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public class DriverPerformance : BaseEntity
{
    public PerformancePeriod Period { get; set; } = PerformancePeriod.Monthly;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal SafetyScore { get; set; } = 0; // Out of 100
    public decimal FuelEfficiencyScore { get; set; } = 0; // Out of 100
    public decimal OnTimePerformanceScore { get; set; } = 0; // Out of 100
    public decimal CustomerSatisfactionScore { get; set; } = 0; // Out of 100
    public decimal OverallScore { get; set; } = 0; // Out of 100
    public int TotalTrips { get; set; } = 0;
    public int CompletedTrips { get; set; } = 0;
    public int CancelledTrips { get; set; } = 0;
    public int LateTrips { get; set; } = 0;
    public decimal TotalMiles { get; set; } = 0;
    public decimal FuelConsumption { get; set; } = 0;
    public decimal FuelCost { get; set; } = 0;
    public int AccidentCount { get; set; } = 0;
    public int ViolationCount { get; set; } = 0;
    public int ComplaintCount { get; set; } = 0;
    public int ComplimentCount { get; set; } = 0;
    public decimal Revenue { get; set; } = 0;
    public decimal Expenses { get; set; } = 0;
    public decimal HoursWorked { get; set; } = 0;
    public decimal OvertimeHours { get; set; } = 0;
    public string Goals { get; set; } = string.Empty; // JSON array
    public string Achievements { get; set; } = string.Empty; // JSON array
    public string AreasForImprovement { get; set; } = string.Empty; // JSON array
    public string ManagerComments { get; set; } = string.Empty;
    public string DriverComments { get; set; } = string.Empty;
    public DateTime? ReviewDate { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}