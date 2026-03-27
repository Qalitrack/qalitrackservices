namespace TechnicianApi.Core.DTOs.Trip;

public class TripResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string TruckId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string? TripTypeId { get; set; }
    public string? CustomTripType { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public double? StartLocationLatitude { get; set; }
    public double? StartLocationLongitude { get; set; }
    public double? EndLocationLatitude { get; set; }
    public double? EndLocationLongitude { get; set; }
    public decimal? StartMileage { get; set; }
    public decimal? EndMileage { get; set; }
    public decimal? TotalMileage { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? ProofEndImageUrl { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialVariantId { get; set; }
    public decimal? MaterialCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
