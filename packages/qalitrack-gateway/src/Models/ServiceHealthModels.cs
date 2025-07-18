namespace QaliTrackGateway.Models;

/// <summary>
/// Real-time service health status
/// </summary>
public class ServiceHealthStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Healthy, Unhealthy, Degraded, Unknown
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string HealthCheckUrl { get; set; } = string.Empty;
    public int ConsecutiveFailures { get; set; }
    public DateTime? LastHealthyTime { get; set; }
    public double HealthScore { get; set; } // 0-100
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    public string Version { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string[] Tags { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Service health check record
/// </summary>
public class ServiceHealthCheck
{
    public string CheckId { get; set; } = Guid.NewGuid().ToString();
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    public string CheckType { get; set; } = "HTTP"; // HTTP, TCP, Custom
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// System-wide health score
/// </summary>
public class SystemHealthScore
{
    public double OverallScore { get; set; } // 0-100
    public int TotalServices { get; set; }
    public int HealthyServices { get; set; }
    public int UnhealthyServices { get; set; }
    public int DegradedServices { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public List<string> CriticalIssues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public double AverageResponseTime { get; set; }
    public string SystemStatus { get; set; } = string.Empty; // Healthy, Degraded, Critical
}

/// <summary>
/// Service dependency mapping
/// </summary>
public class ServiceDependencyMap
{
    public List<ServiceNode> Services { get; set; } = new();
    public List<ServiceDependency> Dependencies { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Service node in dependency graph
/// </summary>
public class ServiceNode
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Gateway, Microservice, Database, External
    public string[] Tags { get; set; } = Array.Empty<string>();
    public bool IsCritical { get; set; }
}

/// <summary>
/// Service dependency relationship
/// </summary>
public class ServiceDependency
{
    public string FromService { get; set; } = string.Empty;
    public string ToService { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty; // API, Database, Queue, Cache
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Service registration information
/// </summary>
public class ServiceRegistration
{
    public string ServiceName { get; set; } = string.Empty;
    public string HealthCheckUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int HealthCheckIntervalSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 10;
    public int MaxConsecutiveFailures { get; set; } = 3;
    public bool IsEnabled { get; set; } = true;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = "development";
}

/// <summary>
/// Service performance metrics
/// </summary>
public class ServicePerformanceMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public double AverageResponseTime { get; set; }
    public double MinResponseTime { get; set; }
    public double MaxResponseTime { get; set; }
    public double SuccessRate { get; set; } // 0-100
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ResponseTimePoint> ResponseTimeHistory { get; set; } = new();
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Response time data point
/// </summary>
public class ResponseTimePoint
{
    public DateTime Timestamp { get; set; }
    public double ResponseTime { get; set; }
    public string Status { get; set; } = string.Empty;
}