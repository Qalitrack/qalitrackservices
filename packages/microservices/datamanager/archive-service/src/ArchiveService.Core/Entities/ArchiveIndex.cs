using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ArchiveService.Core.Entities;

public class ArchiveIndex : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ArchiveId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string IndexId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty; // TRANSACTIONS, WEIGHT_MEASUREMENTS, COMPLIANCE_RECORDS
    
    [Required]
    public IndexType IndexType { get; set; } = IndexType.ElasticSearch;
    
    [Required]
    [StringLength(100)]
    public string IndexName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(1000)]
    public string IndexFields { get; set; } = string.Empty; // JSON array of indexed fields
    
    [StringLength(2000)]
    public string? IndexConfiguration { get; set; } // JSON configuration
    
    public IndexStatus Status { get; set; } = IndexStatus.Active;
    
    public DateTime? LastRebuiltAt { get; set; }
    
    public long DocumentCount { get; set; } = 0; // Number of documents in index
    public long IndexSizeBytes { get; set; } = 0; // Index size in bytes
    
    [StringLength(500)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(1000)]
    public string? ErrorDetails { get; set; }
    
    public decimal AverageSearchTimeMs { get; set; } = 0; // Average search time in milliseconds
    public long TotalSearches { get; set; } = 0; // Total number of searches performed
    
    [StringLength(100)]
    public string Version { get; set; } = "1.0";
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    // Performance Metrics
    public decimal? SearchThroughputPerSecond { get; set; }
    public decimal? IndexingThroughputPerSecond { get; set; }
    public long? MemoryUsageBytes { get; set; }
    public decimal? CpuUsagePercentage { get; set; }
    
    // Maintenance Information
    public DateTime? LastOptimizedAt { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public bool RequiresMaintenance { get; set; } = false;
    
    // JSON properties for flexible configuration
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? IndexFieldsList
    {
        get => string.IsNullOrEmpty(IndexFields) ? null : JsonConvert.DeserializeObject<List<string>>(IndexFields);
        set => IndexFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? IndexConfigurationObject
    {
        get => string.IsNullOrEmpty(IndexConfiguration) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(IndexConfiguration);
        set => IndexConfiguration = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsHealthy()
    {
        return Status == IndexStatus.Active && string.IsNullOrEmpty(ErrorMessage);
    }
    
    public string GetHumanReadableSize()
    {
        var bytes = IndexSizeBytes;
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes = bytes / 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }
    
    public decimal GetSearchEfficiency()
    {
        if (TotalSearches == 0) return 0;
        return AverageSearchTimeMs > 0 ? 1000 / AverageSearchTimeMs : 0; // Searches per second
    }
    
    // Navigation properties
    public virtual Archive Archive { get; set; } = null!;
    public virtual ICollection<ArchiveMetadata> ArchiveMetadata { get; set; } = new List<ArchiveMetadata>();
}

public enum IndexType
{
    ElasticSearch = 1,
    Lucene = 2,
    Solr = 3,
    Database = 4,
    Custom = 5
}

public enum IndexStatus
{
    Active = 1,
    Building = 2,
    Rebuilding = 3,
    Optimizing = 4,
    Corrupted = 5,
    Disabled = 6,
    Failed = 7
}