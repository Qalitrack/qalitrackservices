using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class WeightLimits
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty; // TRUCK, TRAILER, etc.
        
        [StringLength(50)]
        public string VehicleClass { get; set; } = string.Empty; // CLASS_1, CLASS_2, etc.
        
        public decimal MaxGrossWeight { get; set; } // in KG
        public decimal MaxAxleWeight { get; set; } // in KG
        public decimal MaxFrontAxleWeight { get; set; } // in KG
        public decimal MaxRearAxleWeight { get; set; } // in KG
        
        public int MaxAxleCount { get; set; }
        
        [StringLength(100)]
        public string WeighbridgeId { get; set; } = string.Empty; // Specific to weighbridge if applicable
        
        [StringLength(100)]
        public string RouteId { get; set; } = string.Empty; // Specific to route if applicable
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public decimal OverweightPenaltyRate { get; set; } = 5.0m; // KES per KG
        
        [StringLength(10)]
        public string PenaltyCurrency { get; set; } = "KES";
        
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(500)]
        public string RegulatoryReference { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
    }
}