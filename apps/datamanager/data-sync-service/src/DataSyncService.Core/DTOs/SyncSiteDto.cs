using DataSyncService.Core.Enums;

namespace DataSyncService.Core.DTOs;

public class SyncSiteDto
{
    public int Id { get; set; }
    public string SiteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public SiteStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public DateTime LastHealthCheck { get; set; }
    public DateTime? LastSyncTime { get; set; }
    public bool IsMain { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSyncSiteRequest
{
    public string SiteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int Priority { get; set; }
    public string? ConfigurationJson { get; set; }
}

public class UpdateSyncSiteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string ApiEndpoint { get; set; } = string.Empty;
    public SiteStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int Priority { get; set; }
    public string? ConfigurationJson { get; set; }
}