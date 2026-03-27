namespace TechnicianApi.Core.DTOs.Fleet;

public class MaterialCostResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string? Location { get; set; }
    public string? UserId { get; set; }
    public bool Synced { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
