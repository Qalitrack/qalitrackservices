using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class ComplianceRule
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string RuleType { get; set; } = string.Empty; // WEIGHT, LICENSE, ROUTE, PRODUCT, ENVIRONMENTAL, OPERATIONAL
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        public string ConfigurationJson { get; set; } = string.Empty; // JSON configuration for rule parameters
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string Severity { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
        
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        
        [StringLength(10)]
        public string Unit { get; set; } = string.Empty; // KG, HOURS, etc.
        
        public decimal? PenaltyAmount { get; set; }
        
        [StringLength(10)]
        public string PenaltyCurrency { get; set; } = "KES";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
        public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
    }
}