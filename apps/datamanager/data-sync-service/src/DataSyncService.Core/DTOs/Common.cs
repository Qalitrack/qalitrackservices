namespace DataSyncService.Core.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}

public class PagingRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class SyncStatusSummary
{
    public int ActiveSessions { get; set; }
    public int PendingSessions { get; set; }
    public int FailedSessions { get; set; }
    public int TotalSites { get; set; }
    public int ActiveSites { get; set; }
    public int UnresolvedConflicts { get; set; }
    public DateTime LastSyncTime { get; set; }
    public Dictionary<string, int> SyncStatistics { get; set; } = new();
}

public class HealthCheckDto
{
    public string SiteId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckTime { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsHealthy { get; set; }
}