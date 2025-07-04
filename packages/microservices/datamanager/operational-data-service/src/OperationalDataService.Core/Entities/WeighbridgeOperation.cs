using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class WeighbridgeOperation : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string WeighbridgeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public OperationalStatus Status { get; set; } = OperationalStatus.Active;
    
    [Required]
    public int MaxHourlyCapacity { get; set; }
    
    [Required]
    public int CurrentLoad { get; set; } = 0;
    
    [Required]
    public decimal UtilizationRate { get; set; } = 0;
    
    [Required]
    public DateTime LastMaintenanceDate { get; set; }
    
    public DateTime? NextMaintenanceDate { get; set; }
    
    [Required]
    public TimeSpan OperatingHours { get; set; }
    
    [MaxLength(1000)]
    public string? OperationalNotes { get; set; }
    
    public List<string> QueuedVehicles { get; set; } = new();
    
    public decimal EstimatedWaitTime { get; set; } = 0;
    
    public DateTime? NextAvailableSlot { get; set; }
}

public enum OperationalStatus
{
    Active,
    Inactive,
    Maintenance,
    OutOfService,
    Offline
}