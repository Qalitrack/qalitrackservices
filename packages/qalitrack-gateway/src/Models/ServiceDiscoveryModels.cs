namespace QaliTrackGateway.Models;

/// <summary>
/// Service instance information for routing
/// </summary>
public class ServiceInstance
{
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    public string ServiceName { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Protocol { get; set; } = "http";
    public bool IsHealthy { get; set; } = true;
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
    public long ResponseTimeMs { get; set; }
    public int Weight { get; set; } = 100; // For load balancing
    public int Priority { get; set; } = 1; // Lower number = higher priority
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active, Inactive, Draining
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = "development";
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Full URL for this instance
    /// </summary>
    public string BaseUrl => $"{Protocol}://{Host}:{Port}";
    
    /// <summary>
    /// Health score for routing decisions (0-100)
    /// </summary>
    public double HealthScore { get; set; } = 100.0;
    
    /// <summary>
    /// Number of consecutive failures
    /// </summary>
    public int ConsecutiveFailures { get; set; } = 0;
}

/// <summary>
/// Service discovery information
/// </summary>
public class ServiceDiscoveryInfo
{
    public Dictionary<string, List<ServiceInstance>> Services { get; set; } = new();
    public int TotalInstances { get; set; }
    public int HealthyInstances { get; set; }
    public int UnhealthyInstances { get; set; }
    public DateTime LastDiscoveryRun { get; set; } = DateTime.UtcNow;
    public List<string> DiscoveredServices { get; set; } = new();
    public List<string> FailedDiscoveries { get; set; } = new();
    public Dictionary<string, CircuitBreakerStatus> CircuitBreakers { get; set; } = new();
}

/// <summary>
/// Circuit breaker status for a service
/// </summary>
public class CircuitBreakerStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsOpen { get; set; } = false;
    public DateTime? OpenedAt { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public int FailureCount { get; set; } = 0;
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public string? LastFailureReason { get; set; }
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;
}

/// <summary>
/// Circuit breaker states
/// </summary>
public enum CircuitBreakerState
{
    Closed,    // Normal operation
    Open,      // Failing - requests are blocked
    HalfOpen   // Testing - allowing limited requests
}

/// <summary>
/// Load balancing algorithm types
/// </summary>
public enum LoadBalancingAlgorithm
{
    RoundRobin,
    LeastConnections,
    WeightedRoundRobin,
    HealthBased,
    ResponseTime
}

/// <summary>
/// Service routing configuration
/// </summary>
public class ServiceRoutingConfig
{
    public string ServiceName { get; set; } = string.Empty;
    public LoadBalancingAlgorithm Algorithm { get; set; } = LoadBalancingAlgorithm.HealthBased;
    public bool EnableHealthCheck { get; set; } = true;
    public bool EnableCircuitBreaker { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableFailover { get; set; } = true;
    public string[] PreferredTags { get; set; } = Array.Empty<string>();
    public Dictionary<string, object> AdditionalSettings { get; set; } = new();
}

/// <summary>
/// Service discovery configuration
/// </summary>
public class ServiceDiscoveryConfig
{
    public bool EnableAutoDiscovery { get; set; } = false;
    public string[] DiscoveryPatterns { get; set; } = Array.Empty<string>();
    public TimeSpan DiscoveryInterval { get; set; } = TimeSpan.FromMinutes(5);
    public string[] DiscoveryEndpoints { get; set; } = Array.Empty<string>();
    public bool EnableConsulIntegration { get; set; } = false;
    public string ConsulEndpoint { get; set; } = "http://localhost:8500";
    public bool EnableKubernetesIntegration { get; set; } = false;
    public string KubernetesNamespace { get; set; } = "default";
    public Dictionary<string, ServiceRoutingConfig> ServiceConfigs { get; set; } = new();
}