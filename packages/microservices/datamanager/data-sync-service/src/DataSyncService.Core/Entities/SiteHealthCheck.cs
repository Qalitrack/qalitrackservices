using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class SiteHealthCheck : BaseEntity
{
    public string HealthCheckId { get; set; } = string.Empty;
    public string SiteId { get; set; } = string.Empty;
    public SiteStatus Status { get; set; }
    public DateTime CheckTime { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    public string CheckType { get; set; } = string.Empty; // Ping, Database, API, etc.
    public bool IsHealthy { get; set; }
    public string? MetadataJson { get; set; }
    
    // Navigation properties
    public virtual SyncSite SyncSite { get; set; } = null!;
}