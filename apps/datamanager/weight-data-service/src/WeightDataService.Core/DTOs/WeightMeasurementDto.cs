using WeightDataService.Core.Entities;

namespace WeightDataService.Core.DTOs;

public class WeightMeasurementDto
{
    public Guid Id { get; set; }
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public string? DriverId { get; set; }
    public decimal Weight { get; set; }
    public MeasurementType Type { get; set; }
    public MeasurementStatus Status { get; set; }
    public DateTime MeasurementDateTime { get; set; }
    public string? TicketReference { get; set; }
    public string? Notes { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string? ProductType { get; set; }
    public string? CustomerReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    
    public List<WeightCorrectionDto> Corrections { get; set; } = new();
}

public class CreateWeightMeasurementDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public string? DriverId { get; set; }
    public decimal Weight { get; set; }
    public MeasurementType Type { get; set; }
    public string? TicketReference { get; set; }
    public string? Notes { get; set; }
    public decimal? TareWeight { get; set; }
    public string? ProductType { get; set; }
    public string? CustomerReference { get; set; }
}

public class UpdateWeightMeasurementDto
{
    public decimal? Weight { get; set; }
    public MeasurementStatus? Status { get; set; }
    public string? Notes { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string? ProductType { get; set; }
    public string? CustomerReference { get; set; }
}