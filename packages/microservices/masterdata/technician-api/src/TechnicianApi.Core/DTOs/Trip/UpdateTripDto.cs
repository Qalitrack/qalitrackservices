namespace TechnicianApi.Core.DTOs.Trip;

public class UpdateTripDto
{
    public string? TripTypeId { get; set; }
    public string? CustomTripType { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public double? StartLocationLatitude { get; set; }
    public double? StartLocationLongitude { get; set; }
    public double? EndLocationLatitude { get; set; }
    public double? EndLocationLongitude { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialVariantId { get; set; }
    public decimal? MaterialCost { get; set; }
}
