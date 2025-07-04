using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class RegulatoryStandard
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string StandardType { get; set; } = string.Empty; // WEIGHT, EMISSION, SAFETY, LICENSE
        
        [StringLength(100)]
        public string RegulatoryBody { get; set; } = string.Empty; // NTSA, NEMA, etc.
        
        [StringLength(50)]
        public string Country { get; set; } = "KENYA";
        
        [StringLength(50)]
        public string Region { get; set; } = string.Empty;
        
        [Required]
        public string StandardDetails { get; set; } = string.Empty; // JSON with standard specifications
        
        public bool IsActive { get; set; } = true;
        
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(100)]
        public string Version { get; set; } = "1.0";
        
        [StringLength(500)]
        public string DocumentReference { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<ComplianceRule> ComplianceRules { get; set; } = new List<ComplianceRule>();
    }
}