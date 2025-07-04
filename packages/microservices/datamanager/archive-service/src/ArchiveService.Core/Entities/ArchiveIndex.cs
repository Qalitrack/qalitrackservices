using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class ArchiveIndex
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string IndexId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // TRANSACTIONS, WEIGHT_MEASUREMENTS, COMPLIANCE_RECORDS
        
        [Required]
        [StringLength(100)]
        public string IndexType { get; set; } = string.Empty; // ELASTICSEARCH, LUCENE, SOLR, CUSTOM
        
        [Required]
        [StringLength(100)]
        public string IndexName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string IndexFields { get; set; } = string.Empty; // JSON array of indexed fields
        
        [StringLength(2000)]
        public string IndexConfiguration { get; set; } = string.Empty; // JSON configuration
        
        [StringLength(50)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, REBUILDING, CORRUPTED, DISABLED
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRebuiltAt { get; set; }
        
        public int DocumentCount { get; set; } // Number of documents in index
        public long IndexSize { get; set; } // Index size in bytes
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string ErrorDetails { get; set; } = string.Empty;
        
        public decimal AverageSearchTime { get; set; } // Average search time in milliseconds
        public long TotalSearches { get; set; } // Total number of searches performed
        
        [StringLength(100)]
        public string Version { get; set; } = "1.0";
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public int ArchiveMetadataId { get; set; }
        public virtual ArchiveMetadata ArchiveMetadata { get; set; } = null!;
    }
}