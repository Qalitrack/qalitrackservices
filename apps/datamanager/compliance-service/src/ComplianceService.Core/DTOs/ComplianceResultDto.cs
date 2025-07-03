namespace ComplianceService.Core.DTOs
{
    public class ComplianceResultDto
    {
        public bool IsCompliant { get; set; }
        public string Status { get; set; } = string.Empty; // COMPLIANT, NON_COMPLIANT, WARNING
        public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
        public List<ComplianceViolationDto> Violations { get; set; } = new();
        public List<ComplianceWarningDto> Warnings { get; set; } = new();
        public decimal TotalPenalty { get; set; }
        public string PenaltyCurrency { get; set; } = "KES";
        public string Summary { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public string CheckedBy { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}