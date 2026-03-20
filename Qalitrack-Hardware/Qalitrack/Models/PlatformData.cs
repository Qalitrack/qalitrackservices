namespace Qalitrack.Models;

public class PlatformData
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<int, string> PlatformWeights { get; set; } = new();
    public string? TotalWeight { get; set; }
}
