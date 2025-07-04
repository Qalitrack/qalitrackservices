using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class LicenseMonitoring
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DriverId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DriverName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string LicenseType { get; set; } = string.Empty; // CLASS_A, CLASS_B, etc.
        
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        [StringLength(100)]
        public string IssuingAuthority { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, EXPIRED, SUSPENDED, REVOKED
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        public bool IsMonitored { get; set; } = true;
        
        public DateTime? LastChecked { get; set; }
        public DateTime? NextCheckDue { get; set; }
        
        public int ExpiryWarningDays { get; set; } = 30; // Days before expiry to warn
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Calculated properties
        public int DaysToExpiry => (ExpiryDate - DateTime.UtcNow).Days;
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
        public bool IsExpiringSoon => DaysToExpiry <= ExpiryWarningDays && DaysToExpiry > 0;
        
        // Navigation properties
        public virtual ICollection<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
    }
}