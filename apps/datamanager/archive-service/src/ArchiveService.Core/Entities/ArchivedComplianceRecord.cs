using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class ArchivedComplianceRecord
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string OriginalComplianceId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string VehicleId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DriverId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ComplianceType { get; set; } = string.Empty; // WEIGHT, LICENSE, ROUTE, PRODUCT, ENVIRONMENTAL
        
        [Required]
        [StringLength(50)]
        public string ComplianceStatus { get; set; } = string.Empty; // COMPLIANT, VIOLATION, WARNING, UNKNOWN
        
        [StringLength(50)]
        public string Severity { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
        
        [StringLength(100)]
        public string ViolationType { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Details { get; set; } = string.Empty;
        
        public decimal? ActualValue { get; set; }
        public decimal? LimitValue { get; set; }
        public decimal? ExcessValue { get; set; }
        
        [StringLength(10)]
        public string Unit { get; set; } = string.Empty;
        
        public decimal? PenaltyAmount { get; set; }
        
        [StringLength(10)]
        public string PenaltyCurrency { get; set; } = "KES";
        
        public DateTime OriginalDetectionDate { get; set; }
        public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string ResolutionStatus { get; set; } = string.Empty;
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(100)]
        public string ResolvedBy { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string ResolutionNotes { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ArchivedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public int ArchiveMetadataId { get; set; }
        public virtual ArchiveMetadata ArchiveMetadata { get; set; } = null!;
        
        public int? ArchivedTransactionId { get; set; }
        public virtual ArchivedTransaction? ArchivedTransaction { get; set; }
    }
}