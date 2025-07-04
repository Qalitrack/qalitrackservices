using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class RouteRestrictions
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string RouteId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string RestrictionType { get; set; } = string.Empty; // WEIGHT, TIME, VEHICLE_TYPE, PRODUCT
        
        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ProductType { get; set; } = string.Empty;
        
        public decimal? MaxWeight { get; set; } // in KG
        public decimal? MaxHeight { get; set; } // in meters
        public decimal? MaxWidth { get; set; } // in meters
        public decimal? MaxLength { get; set; } // in meters
        
        public TimeSpan? RestrictedFromTime { get; set; }
        public TimeSpan? RestrictedToTime { get; set; }
        
        public bool IsWeekdaysOnly { get; set; } = false;
        public bool IsWeekendsOnly { get; set; } = false;
        
        [StringLength(100)]
        public string DaysOfWeek { get; set; } = string.Empty; // JSON array of days
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public decimal? ViolationPenalty { get; set; }
        
        [StringLength(10)]
        public string PenaltyCurrency { get; set; } = "KES";
        
        [StringLength(50)]
        public string Severity { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
        
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(500)]
        public string RegulatoryReference { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string AdditionalDetails { get; set; } = string.Empty; // JSON with additional restrictions
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Helper methods
        public bool IsTimeRestricted => RestrictedFromTime.HasValue && RestrictedToTime.HasValue;
        public bool IsCurrentlyRestricted => IsTimeRestricted && 
            DateTime.Now.TimeOfDay >= RestrictedFromTime && 
            DateTime.Now.TimeOfDay <= RestrictedToTime;
        
        // Navigation properties
        public virtual ICollection<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
    }
}