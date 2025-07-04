namespace ComplianceService.Core.DTOs
{
    public class ComplianceViolationDto
    {
        public int Id { get; set; }
        public int ComplianceRuleId { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string DriverId { get; set; } = string.Empty;
        public string WeighbridgeId { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public decimal? ActualValue { get; set; }
        public decimal? LimitValue { get; set; }
        public decimal? ExcessValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? PenaltyAmount { get; set; }
        public string PenaltyCurrency { get; set; } = "KES";
        public string Description { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
        public string ResolutionNotes { get; set; } = string.Empty;
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string RuleCategory { get; set; } = string.Empty;
    }
}