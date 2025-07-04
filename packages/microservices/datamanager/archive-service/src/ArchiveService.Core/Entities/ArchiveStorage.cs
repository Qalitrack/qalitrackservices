using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class ArchiveStorage
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string StorageType { get; set; } = string.Empty; // LOCAL, AZURE_BLOB, AWS_S3, GOOGLE_CLOUD
        
        [StringLength(50)]
        public string StorageTier { get; set; } = "HOT"; // HOT, WARM, COLD
        
        [Required]
        [StringLength(500)]
        public string ConnectionString { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ContainerName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string BasePath { get; set; } = string.Empty;
        
        public long MaxStorageSize { get; set; } // Maximum storage in bytes (0 = unlimited)
        public long CurrentStorageSize { get; set; } // Current storage used in bytes
        
        public decimal CostPerGB { get; set; } // Cost per GB
        
        [StringLength(10)]
        public string Currency { get; set; } = "USD";
        
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string AvailabilityZone { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ReplicationLevel { get; set; } = "NONE"; // NONE, LOCAL, ZONE, REGION
        
        [StringLength(50)]
        public string EncryptionType { get; set; } = "AES256"; // AES256, AES128, NONE
        
        [StringLength(500)]
        public string EncryptionKey { get; set; } = string.Empty;
        
        public DateTime? LastHealthCheck { get; set; }
        
        [StringLength(50)]
        public string HealthStatus { get; set; } = "UNKNOWN"; // HEALTHY, DEGRADED, UNHEALTHY, UNKNOWN
        
        [StringLength(500)]
        public string HealthDetails { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string ConfigurationJson { get; set; } = string.Empty; // Additional configuration as JSON
        
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
    }
}