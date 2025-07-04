namespace QaliTrack.Gateway.Models;

public class ClientConfiguration
{
    public ClientInfo Client { get; set; } = new();
    public DeploymentInfo Deployment { get; set; } = new();
    public Dictionary<string, ServiceInfo> Services { get; set; } = new();
    public FeatureFlags Features { get; set; } = new();
}

public class ClientInfo
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DeploymentInfo
{
    public string Environment { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public PortConfiguration Ports { get; set; } = new();
}

public class PortConfiguration
{
    public int Gateway { get; set; }
    public int SwaggerAggregator { get; set; }
}

public class ServiceInfo
{
    public bool Enabled { get; set; }
    public int Port { get; set; }
    public string Image { get; set; } = string.Empty;
    public int Replicas { get; set; } = 1;
    public string? Reason { get; set; }
    public bool TestMode { get; set; }
    public bool ReadOnly { get; set; }
}

public class FeatureFlags
{
    public bool MultiTenant { get; set; }
    public bool AdvancedAnalytics { get; set; }
    public bool ComplianceMonitoring { get; set; }
    public bool RealTimeWeighing { get; set; }
    public bool AutomatedReporting { get; set; }
    public bool MobileAccess { get; set; }
    public bool TestingMode { get; set; }
}

public class SwaggerEndpoint
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public bool Available { get; set; } = true;
}