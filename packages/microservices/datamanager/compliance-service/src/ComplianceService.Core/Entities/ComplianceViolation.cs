using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class ComplianceViolation
    {
        public int Id { get; set; }
        
        public int ComplianceRuleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ViolationType { get; set; } = string.Empty; // WEIGHT_EXCEEDED, LICENSE_EXPIRED, etc.
        
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string VehicleId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DriverId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string WeighbridgeId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, RESOLVED, DISPUTED, DISMISSED
        
        [StringLength(50)]
        public string Severity { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
        
        public decimal? ActualValue { get; set; }
        public decimal? LimitValue { get; set; }
        public decimal? ExcessValue { get; set; }
        
        [StringLength(10)]
        public string Unit { get; set; } = string.Empty;
        
        public decimal? PenaltyAmount { get; set; }
        
        [StringLength(10)]
        public string PenaltyCurrency { get; set; } = "KES";
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Details { get; set; } = string.Empty; // JSON with additional violation details
        
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        
        [StringLength(100)]
        public string ResolvedBy { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string ResolutionNotes { get; set; } = string.Empty;
        
        public bool IsAcknowledged { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
        
        [StringLength(100)]
        public string AcknowledgedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ComplianceRule ComplianceRule { get; set; } = null!;
        public virtual ICollection<ComplianceAudit> AuditEntries { get; set; } = new List<ComplianceAudit>();
    }
}