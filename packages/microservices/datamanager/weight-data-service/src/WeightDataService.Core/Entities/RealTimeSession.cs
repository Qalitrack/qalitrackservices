namespace WeightDataService.Core.Entities;

public class RealTimeSession : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public StreamingStatus Status { get; set; } = StreamingStatus.Active;
    public int SampleRate { get; set; } = 1000; // milliseconds
    public decimal CurrentWeight { get; set; }
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal AverageWeight { get; set; }
    public int SampleCount { get; set; }
    public bool IsStable { get; set; } = false;
    public decimal StabilityThreshold { get; set; } = 0.1m;
    public string? EventData { get; set; } // JSON for additional events
    public string OrganizationId { get; set; } = string.Empty;
    
    public ICollection<WeightMeasurement> Measurements { get; set; } = new List<WeightMeasurement>();
}

public enum StreamingStatus
{
    Active,
    Paused,
    Completed,
    Error,
    Cancelled
}