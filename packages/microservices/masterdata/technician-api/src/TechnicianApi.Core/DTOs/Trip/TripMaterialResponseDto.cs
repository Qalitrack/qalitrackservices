namespace TechnicianApi.Core.DTOs.Trip;

public class TripMaterialResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string TripId { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public string? MaterialVariantId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime AddedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
