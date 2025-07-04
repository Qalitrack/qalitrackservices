using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class DataMigration
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MigrationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // TRANSACTIONS, WEIGHT_MEASUREMENTS, COMPLIANCE_RECORDS
        
        [Required]
        [StringLength(50)]
        public string MigrationType { get; set; } = string.Empty; // ARCHIVE, RESTORE, DELETE, MIGRATE
        
        [StringLength(50)]
        public string Status { get; set; } = "PENDING"; // PENDING, RUNNING, COMPLETED, FAILED, CANCELLED
        
        public DateTime StartDate { get; set; } // Start date of data being migrated
        public DateTime EndDate { get; set; } // End date of data being migrated
        
        public int TotalRecords { get; set; } // Total records to migrate
        public int ProcessedRecords { get; set; } // Records processed so far
        public int SuccessfulRecords { get; set; } // Records migrated successfully
        public int FailedRecords { get; set; } // Records that failed to migrate
        
        public decimal ProgressPercentage { get; set; } // Progress percentage
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string ErrorDetails { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string MigrationCriteria { get; set; } = string.Empty; // JSON criteria used for migration
        
        [StringLength(500)]
        public string SourceLocation { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string DestinationLocation { get; set; } = string.Empty;
        
        public long? EstimatedDuration { get; set; } // Estimated duration in seconds
        public long? ActualDuration { get; set; } // Actual duration in seconds
        
        [StringLength(100)]
        public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public int? RetentionPolicyId { get; set; }
        public virtual RetentionPolicy? RetentionPolicy { get; set; }
    }
}