namespace WeightDataService.Core.Entities;

public class WeightMeasurement : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public string? DriverId { get; set; }
    public decimal Weight { get; set; }
    public MeasurementType Type { get; set; }
    public MeasurementStatus Status { get; set; } = MeasurementStatus.Pending;
    public DateTime MeasurementDateTime { get; set; } = DateTime.UtcNow;
    public string? TicketReference { get; set; }
    public string? Notes { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string? ProductType { get; set; }
    public string? CustomerReference { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<WeightCorrection> Corrections { get; set; } = new List<WeightCorrection>();
}