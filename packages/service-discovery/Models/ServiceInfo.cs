namespace ServiceDiscovery.Models;

public class ServiceInfo
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public List<EndpointInfo> Endpoints { get; set; } = new();
    public bool IsHealthy { get; set; } = false;
    public string? Status { get; set; }
}

public class EndpointInfo
{
    public string Path { get; set; } = "";
    public string Method { get; set; } = "";
    public string Description { get; set; } = "";
}

public class GatewayServicesResponse
{
    public List<GatewayService> Services { get; set; } = new();
}

public class GatewayService
{
    public string? Name { get; set; }
    public string? BaseUrl { get; set; }
    public string? HealthUrl { get; set; }
    public bool IsHealthy { get; set; }
}

public class ServiceDiscoveryResponse
{
    public string Title { get; set; } = "QaliTrack Services Discovery";
    public string Description { get; set; } = "Available services and endpoints";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<ServiceInfo> Services { get; set; } = new();
    public ServiceStatistics Statistics { get; set; } = new();
}

public class ServiceStatistics
{
    public int TotalServices { get; set; }
    public int TotalEndpoints { get; set; }
    public int HealthyServices { get; set; }
    public int Categories { get; set; }
}