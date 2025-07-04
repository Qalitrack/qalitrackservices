using DriverService.Core.Entities;

namespace DriverService.Core.DTOs;

public class DriverPerformanceDto
{
    public string Id { get; set; } = string.Empty;
    public PerformancePeriod Period { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal SafetyScore { get; set; }
    public decimal FuelEfficiencyScore { get; set; }
    public decimal OnTimePerformanceScore { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }
    public decimal OverallScore { get; set; }
    public int TotalTrips { get; set; }
    public int CompletedTrips { get; set; }
    public int CancelledTrips { get; set; }
    public int LateTrips { get; set; }
    public decimal TotalMiles { get; set; }
    public decimal FuelConsumption { get; set; }
    public decimal FuelCost { get; set; }
    public int AccidentCount { get; set; }
    public int ViolationCount { get; set; }
    public int ComplaintCount { get; set; }
    public int ComplimentCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Goals { get; set; } = string.Empty;
    public string Achievements { get; set; } = string.Empty;
    public string AreasForImprovement { get; set; } = string.Empty;
    public string ManagerComments { get; set; } = string.Empty;
    public string DriverComments { get; set; } = string.Empty;
    public DateTime? ReviewDate { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDriverPerformanceDto
{
    public PerformancePeriod Period { get; set; } = PerformancePeriod.Monthly;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal SafetyScore { get; set; } = 0;
    public decimal FuelEfficiencyScore { get; set; } = 0;
    public decimal OnTimePerformanceScore { get; set; } = 0;
    public decimal CustomerSatisfactionScore { get; set; } = 0;
    public decimal OverallScore { get; set; } = 0;
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
    public string Goals { get; set; } = string.Empty;
    public string Achievements { get; set; } = string.Empty;
    public string AreasForImprovement { get; set; } = string.Empty;
    public string ManagerComments { get; set; } = string.Empty;
    public string DriverComments { get; set; } = string.Empty;
    public DateTime? ReviewDate { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
}