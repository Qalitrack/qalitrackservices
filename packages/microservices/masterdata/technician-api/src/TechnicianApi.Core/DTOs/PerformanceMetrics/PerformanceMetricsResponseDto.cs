namespace TechnicianApi.Core.DTOs.PerformanceMetrics;

public class PerformanceMetricsResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int DelayedAssignments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal OnTimeRate { get; set; }
    public decimal PerformanceScore { get; set; }
    public string AlertLevel { get; set; } = string.Empty;
    public int AverageCompletionMinutes { get; set; }
    public int TotalWorkingHours { get; set; }
    public int ReportsSubmitted { get; set; }
    public int ReportsApproved { get; set; }
    public int ReportsRejected { get; set; }
    public decimal ReportApprovalRate { get; set; }
    public int TotalRequisitions { get; set; }
    public int ApprovedRequisitions { get; set; }
    public decimal TotalRequisitionAmount { get; set; }
    public int AmberThreshold { get; set; }
    public int RedThreshold { get; set; }
    public DateTime CalculatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
