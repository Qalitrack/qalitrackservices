namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeCapacityDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public decimal CurrentLoad { get; set; }
    public decimal MaxCapacity { get; set; }
    public decimal AvailableCapacity { get; set; }
    public decimal UsagePercentage { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsOverCapacity { get; set; }
    public bool IsUnderMinimum { get; set; }
    public string? AlertLevel { get; set; }
    public int QueueLength { get; set; }
    public decimal EstimatedWaitTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? UnavailableReason { get; set; }
    public DateTime? NextAvailableTime { get; set; }
}

public class UpdateCapacityRequest
{
    public decimal CurrentLoad { get; set; }
    public int QueueLength { get; set; } = 0;
    public decimal EstimatedWaitTime { get; set; } = 0;
    public bool IsAvailable { get; set; } = true;
    public string? UnavailableReason { get; set; }
    public DateTime? NextAvailableTime { get; set; }
}