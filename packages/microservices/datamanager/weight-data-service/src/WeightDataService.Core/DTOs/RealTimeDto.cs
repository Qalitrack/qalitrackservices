namespace WeightDataService.Core.DTOs;

public class RealTimeSessionDto
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SampleRate { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal AverageWeight { get; set; }
    public int SampleCount { get; set; }
    public bool IsStable { get; set; }
    public decimal StabilityThreshold { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
}

public class CreateRealTimeSessionDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public int SampleRate { get; set; } = 1000;
    public decimal StabilityThreshold { get; set; } = 0.1m;
}

public class UpdateRealTimeSessionDto
{
    public decimal CurrentWeight { get; set; }
    public bool IsStable { get; set; }
    public string? EventData { get; set; }
}

public class StreamingWeightDataDto
{
    public string SessionId { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsStable { get; set; }
    public string? EventType { get; set; }
}