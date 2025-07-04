using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class SyncSite : BaseEntity
{
    public string SiteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public SiteStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public DateTime LastHealthCheck { get; set; }
    public DateTime? LastSyncTime { get; set; }
    public string? ConfigurationJson { get; set; }
    public bool IsMain { get; set; }
    public int Priority { get; set; }
    public string? AuthTokenHash { get; set; }
    public DateTime? AuthTokenExpiry { get; set; }
    
    // Navigation properties
    public virtual ICollection<SyncSession> SourceSessions { get; set; } = new List<SyncSession>();
    public virtual ICollection<SyncSession> TargetSessions { get; set; } = new List<SyncSession>();
    public virtual ICollection<SiteHealthCheck> HealthChecks { get; set; } = new List<SiteHealthCheck>();
}