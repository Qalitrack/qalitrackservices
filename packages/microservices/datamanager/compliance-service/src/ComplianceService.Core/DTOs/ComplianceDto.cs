using ComplianceService.Core.Entities;

namespace ComplianceService.Core.DTOs;

public class ComplianceDto
{
    public string Id { get; set; } = string.Empty;
    public string ComplianceName { get; set; } = string.Empty;
    public ComplianceType ComplianceType { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ComplianceStatus Status { get; set; }
    public DateTime CheckDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public CompliancePriority Priority { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public decimal RiskScore { get; set; }
    public string? RiskAssessment { get; set; }
    public string? RegulatoryFramework { get; set; }
    public string? RegulatoryBody { get; set; }
    public string? ComplianceStandardId { get; set; }
    public bool IsCompliant { get; set; }
    public decimal ComplianceScore { get; set; }
    public string? NonComplianceReason { get; set; }
    public string? RecommendedActions { get; set; }
    public string? CompletedActions { get; set; }
    public bool RequiresContinuousMonitoring { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public int? MonitoringFrequencyDays { get; set; }
    public Dictionary<string, object>? CheckResults { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public List<ComplianceEvidence>? Evidence { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CreateComplianceRequest
{
    public string ComplianceName { get; set; } = string.Empty;
    public ComplianceType ComplianceType { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedUserId { get; set; }
    public CompliancePriority Priority { get; set; } = CompliancePriority.Medium;
    public string? RegulatoryFramework { get; set; }
    public string? RegulatoryBody { get; set; }
    public string? ComplianceStandardId { get; set; }
    public bool RequiresContinuousMonitoring { get; set; } = false;
    public int? MonitoringFrequencyDays { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateComplianceRequest
{
    public string? Description { get; set; }
    public ComplianceStatus? Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedUserId { get; set; }
    public CompliancePriority? Priority { get; set; }
    public string? NonComplianceReason { get; set; }
    public string? RecommendedActions { get; set; }
    public string? CompletedActions { get; set; }
    public bool? RequiresContinuousMonitoring { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public int? MonitoringFrequencyDays { get; set; }
    public Dictionary<string, object>? CheckResults { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public List<ComplianceEvidence>? Evidence { get; set; }
}

public class ComplianceCheckRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public List<string>? RuleIds { get; set; }
    public ComplianceType? ComplianceType { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

public class ComplianceSummaryDto
{
    public string OrganizationId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalComplianceChecks { get; set; }
    public int CompliantItems { get; set; }
    public int NonCompliantItems { get; set; }
    public int PartiallyCompliantItems { get; set; }
    public decimal OverallComplianceRate { get; set; }
    public decimal AverageRiskScore { get; set; }
    public int HighRiskItems { get; set; }
    public int CriticalViolations { get; set; }
    public List<ComplianceTypeStatistics> ComplianceByType { get; set; } = new List<ComplianceTypeStatistics>();
    public List<ComplianceStatusStatistics> ComplianceByStatus { get; set; } = new List<ComplianceStatusStatistics>();
}

public class ComplianceTypeStatistics
{
    public ComplianceType ComplianceType { get; set; }
    public int Total { get; set; }
    public int Compliant { get; set; }
    public int NonCompliant { get; set; }
    public decimal ComplianceRate { get; set; }
}

public class ComplianceStatusStatistics
{
    public ComplianceStatus Status { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class RiskAssessmentDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public decimal RiskScore { get; set; }
    public string RiskAssessment { get; set; } = string.Empty;
    public List<RiskFactor> RiskFactors { get; set; } = new List<RiskFactor>();
    public List<string> RecommendedMitigations { get; set; } = new List<string>();
    public DateTime AssessmentDate { get; set; }
}

public class RiskFactor
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Description { get; set; } = string.Empty;
    public RiskLevel Impact { get; set; }
}

public class ComplianceRiskReportDto
{
    public string OrganizationId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OverallRiskScore { get; set; }
    public List<RiskAssessmentDto> HighRiskItems { get; set; } = new List<RiskAssessmentDto>();
    public List<RiskTrendData> RiskTrends { get; set; } = new List<RiskTrendData>();
    public List<RiskCategoryAnalysis> RiskByCategory { get; set; } = new List<RiskCategoryAnalysis>();
}

public class RiskTrendData
{
    public DateTime Date { get; set; }
    public decimal AverageRiskScore { get; set; }
    public int HighRiskCount { get; set; }
    public int CriticalRiskCount { get; set; }
}

public class RiskCategoryAnalysis
{
    public ComplianceType Category { get; set; }
    public decimal AverageRiskScore { get; set; }
    public int TotalItems { get; set; }
    public int HighRiskItems { get; set; }
    public int CriticalRiskItems { get; set; }
}

public class MonitoringScheduleDto
{
    public string OrganizationId { get; set; } = string.Empty;
    public List<ScheduledMonitoring> ScheduledItems { get; set; } = new List<ScheduledMonitoring>();
    public List<OverdueMonitoring> OverdueItems { get; set; } = new List<OverdueMonitoring>();
}

public class ScheduledMonitoring
{
    public string ComplianceId { get; set; } = string.Empty;
    public string ComplianceName { get; set; } = string.Empty;
    public DateTime NextCheckDate { get; set; }
    public int FrequencyDays { get; set; }
    public CompliancePriority Priority { get; set; }
}

public class OverdueMonitoring
{
    public string ComplianceId { get; set; } = string.Empty;
    public string ComplianceName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public CompliancePriority Priority { get; set; }
}