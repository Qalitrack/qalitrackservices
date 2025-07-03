namespace WeighbridgeService.Core.Entities;

public class WeighbridgeCapacity : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public decimal CurrentLoad { get; set; } = 0;
    public decimal MaxCapacity { get; set; }
    public decimal AvailableCapacity { get; set; }
    public decimal UsagePercentage { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsOverCapacity { get; set; } = false;
    public bool IsUnderMinimum { get; set; } = false;
    public string? AlertLevel { get; set; } // Normal, Warning, Critical
    public int QueueLength { get; set; } = 0; // Number of vehicles waiting
    public decimal EstimatedWaitTime { get; set; } = 0; // in minutes
    public bool IsAvailable { get; set; } = true;
    public string? UnavailableReason { get; set; }
    public DateTime? NextAvailableTime { get; set; }
    
    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}