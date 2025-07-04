using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class RetentionPolicy
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // TRANSACTIONS, WEIGHT_MEASUREMENTS, COMPLIANCE_RECORDS
        
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Optional category filter
        
        public int RetentionDays { get; set; } // How long to keep in operational database
        public int ArchiveAfterDays { get; set; } // When to archive data
        public int DeleteAfterDays { get; set; } // When to delete archived data (0 = never delete)
        
        [StringLength(50)]
        public string StorageTier { get; set; } = "HOT"; // HOT, WARM, COLD
        
        [StringLength(50)]
        public string CompressionType { get; set; } = "GZIP";
        
        public bool IsActive { get; set; } = true;
        public bool IsAutomatic { get; set; } = true; // Whether to execute automatically
        
        [StringLength(100)]
        public string ExecutionSchedule { get; set; } = "DAILY"; // DAILY, WEEKLY, MONTHLY
        
        public DateTime? LastExecuted { get; set; }
        public DateTime? NextExecution { get; set; }
        
        [StringLength(1000)]
        public string FilterCriteria { get; set; } = string.Empty; // JSON filter criteria
        
        [StringLength(500)]
        public string OrganizationIds { get; set; } = string.Empty; // Comma-separated org IDs (empty = all)
        
        [StringLength(100)]
        public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<ArchiveMetadata> ArchiveMetadatas { get; set; } = new List<ArchiveMetadata>();
        public virtual ICollection<DataMigration> DataMigrations { get; set; } = new List<DataMigration>();
    }
}