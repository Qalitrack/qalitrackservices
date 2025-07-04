using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class ArchivedTransaction
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string OriginalTransactionId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string TransactionType { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string VehicleId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DriverId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string WeighbridgeId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string CustomerId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ProductId { get; set; } = string.Empty;
        
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        
        [StringLength(10)]
        public string WeightUnit { get; set; } = "KG";
        
        public decimal? TransactionAmount { get; set; }
        
        [StringLength(10)]
        public string Currency { get; set; } = "KES";
        
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        
        public DateTime OriginalTransactionDate { get; set; }
        public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(1000)]
        public string TransactionDetails { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ArchivedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public int ArchiveMetadataId { get; set; }
        public virtual ArchiveMetadata ArchiveMetadata { get; set; } = null!;
        
        public virtual ICollection<ArchivedComplianceRecord> ComplianceRecords { get; set; } = new List<ArchivedComplianceRecord>();
    }
}