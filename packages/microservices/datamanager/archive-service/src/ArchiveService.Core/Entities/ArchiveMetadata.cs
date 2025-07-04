using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class ArchiveMetadata
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ArchiveId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // TRANSACTIONS, WEIGHT_MEASUREMENTS, COMPLIANCE_RECORDS
        
        [Required]
        [StringLength(100)]
        public string ArchiveName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime ArchiveDate { get; set; } = DateTime.UtcNow;
        
        public DateTime StartDate { get; set; } // Start date of data being archived
        public DateTime EndDate { get; set; } // End date of data being archived
        
        public int RecordCount { get; set; } // Number of records in this archive
        
        public long OriginalSize { get; set; } // Original size in bytes
        public long CompressedSize { get; set; } // Compressed size in bytes
        
        [StringLength(50)]
        public string CompressionType { get; set; } = "GZIP";
        
        [StringLength(100)]
        public string ChecksumType { get; set; } = "SHA256";
        
        [StringLength(500)]
        public string ChecksumValue { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, ARCHIVED, DELETED, CORRUPTED
        
        [StringLength(100)]
        public string StorageLocation { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string StorageTier { get; set; } = "HOT"; // HOT, WARM, COLD
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastAccessedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(1000)]
        public string AdditionalMetadata { get; set; } = string.Empty; // JSON metadata
        
        [StringLength(500)]
        public string Tags { get; set; } = string.Empty; // Comma-separated tags
        
        // Navigation properties
        public int? RetentionPolicyId { get; set; }
        public virtual RetentionPolicy? RetentionPolicy { get; set; }
        
        public int? ArchiveStorageId { get; set; }
        public virtual ArchiveStorage? ArchiveStorage { get; set; }
        
        public virtual ICollection<ArchivedTransaction> ArchivedTransactions { get; set; } = new List<ArchivedTransaction>();
        public virtual ICollection<ArchivedWeightMeasurement> ArchivedWeightMeasurements { get; set; } = new List<ArchivedWeightMeasurement>();
        public virtual ICollection<ArchivedComplianceRecord> ArchivedComplianceRecords { get; set; } = new List<ArchivedComplianceRecord>();
        public virtual ICollection<ArchiveIndex> ArchiveIndexes { get; set; } = new List<ArchiveIndex>();
    }
}