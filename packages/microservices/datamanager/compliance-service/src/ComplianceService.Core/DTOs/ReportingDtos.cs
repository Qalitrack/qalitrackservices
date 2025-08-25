namespace ComplianceService.Core.DTOs;

public class ReportingDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class AuditReportDto
{
    public string Id { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; }
    public List<string> Findings { get; set; } = new();
    public Dictionary<string, object> Results { get; set; } = new();
}

public class ComplianceTrendDto
{
    public DateTime Date { get; set; }
    public double ComplianceRate { get; set; }
    public int TotalTransactions { get; set; }
    public int ViolationsCount { get; set; }
}

public class ViolationSummaryDto
{
    public string ViolationType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Severity { get; set; }
    public List<string> Examples { get; set; } = new();
}

public class PerformanceMetricsDto
{
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime MeasuredAt { get; set; }
}

public class ComplianceScoreDto
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime CalculatedAt { get; set; }
    public Dictionary<string, double> CategoryScores { get; set; } = new();
}

public class UpdateReportRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class GenerateComplianceReportRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> ComplianceTypes { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class GenerateViolationReportRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> ViolationTypes { get; set; } = new();
    public string Severity { get; set; } = string.Empty;
}

public class GenerateRiskReportRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public double MinRiskLevel { get; set; }
    public List<string> RiskCategories { get; set; } = new();
}

public class GenerateRegulatorySubmissionRequest
{
    public string RegulatoryId { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public Dictionary<string, object> SubmissionData { get; set; } = new();
}

public class GenerateExecutiveDashboardRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> MetricCategories { get; set; } = new();
}

public class ReportValidationDto
{
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public Dictionary<string, object> ValidationResults { get; set; } = new();
}

public class SubmissionStatusDto
{
    public string SubmissionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StatusDate { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
}

public class ReportAnalyticsDto
{
    public string ReportId { get; set; } = string.Empty;
    public Dictionary<string, object> Analytics { get; set; } = new();
    public List<string> KeyInsights { get; set; } = new();
}

public class TrendAnalysisDto
{
    public string MetricName { get; set; } = string.Empty;
    public double TrendValue { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public List<Dictionary<string, object>> TrendData { get; set; } = new();
}

public class MetricAnalysisDto
{
    public string MetricName { get; set; } = string.Empty;
    public Dictionary<string, object> Analysis { get; set; } = new();
    public List<string> Insights { get; set; } = new();
    public double TrendDirection { get; set; }
}

public class BenchmarkingDto
{
    public string Category { get; set; } = string.Empty;
    public double OurScore { get; set; }
    public double IndustryAverage { get; set; }
    public double BestPractice { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class ScheduleReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> Recipients { get; set; } = new();
}

public class ScheduledReportDto
{
    public string Id { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public DateTime NextRun { get; set; }
    public bool IsActive { get; set; }
}

public class ReportTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class CreateReportTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class RegulatorySubmissionDto
{
    public string Id { get; set; } = string.Empty;
    public string RegulatoryId { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> SubmissionData { get; set; } = new();
}

public class SubmissionHistoryDto
{
    public string SubmissionId { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Changes { get; set; } = new();
}

public class CreateReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}