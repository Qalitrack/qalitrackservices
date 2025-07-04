using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class ComplianceCheck
    {
        public int Id { get; set; }
        
        public int ComplianceRuleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CheckType { get; set; } = string.Empty; // MANUAL, AUTOMATIC, SCHEDULED
        
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string VehicleId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DriverId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // PENDING, PASSED, FAILED, ERROR
        
        [Required]
        [StringLength(50)]
        public string Result { get; set; } = string.Empty; // COMPLIANT, NON_COMPLIANT, WARNING
        
        public decimal? CheckedValue { get; set; }
        public decimal? LimitValue { get; set; }
        
        [StringLength(10)]
        public string Unit { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string CheckDetails { get; set; } = string.Empty; // JSON with check details
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CheckedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ComplianceRule ComplianceRule { get; set; } = null!;
    }
}