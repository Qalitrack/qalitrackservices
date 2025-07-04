namespace ComplianceService.Core.DTOs
{
    public class ComplianceWarningDto
    {
        public string WarningType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "LOW"; // LOW, MEDIUM, HIGH
        public string Category { get; set; } = string.Empty;
        public decimal? CurrentValue { get; set; }
        public decimal? ThresholdValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}