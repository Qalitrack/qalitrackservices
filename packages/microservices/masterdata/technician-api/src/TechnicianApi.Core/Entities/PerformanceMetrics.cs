namespace TechnicianApi.Core.Entities;

public class PerformanceMetrics : BaseEntity
{
    public string TechnicianId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Overall metrics
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int DelayedAssignments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal OnTimeRate { get; set; }

    // Performance score (0-100)
    public decimal PerformanceScore { get; set; }
    public PerformanceAlertLevel AlertLevel { get; set; } = PerformanceAlertLevel.None;

    // Time metrics
    public int AverageCompletionMinutes { get; set; }
    public int TotalWorkingHours { get; set; }

    // Quality metrics
    public int ReportsSubmitted { get; set; }
    public int ReportsApproved { get; set; }
    public int ReportsRejected { get; set; }
    public decimal ReportApprovalRate { get; set; }

    // Financial metrics
    public int TotalRequisitions { get; set; }
    public int ApprovedRequisitions { get; set; }
    public decimal TotalRequisitionAmount { get; set; }

    // Alerts and thresholds
    public int AmberThreshold { get; set; } = 2;
    public int RedThreshold { get; set; } = 4;

    // Calculated at
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
