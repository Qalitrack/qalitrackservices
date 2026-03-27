namespace TechnicianApi.Core.DTOs.Trip;

public class CreateTripMaterialDto
{
    public string TripId { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public string? MaterialVariantId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}
