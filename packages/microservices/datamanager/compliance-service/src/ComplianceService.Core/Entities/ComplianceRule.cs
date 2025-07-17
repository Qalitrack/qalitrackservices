using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ComplianceService.Core.Entities
{
    public class ComplianceRule : BaseEntity
    {
        
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
        
        [StringLength(50)]
        public string? RegulatoryId { get; set; }
        
        // JSON properties for flexible configuration
        public string? MetadataJson { get; set; }
        
        // Computed properties
        public Dictionary<string, object>? Configuration
        {
            get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
            set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
        }
        
        public Dictionary<string, object>? Metadata
        {
            get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
            set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
        }
        
        // Navigation properties
        public virtual ICollection<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
        public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
        public virtual Regulatory? Regulatory { get; set; }
    }
}