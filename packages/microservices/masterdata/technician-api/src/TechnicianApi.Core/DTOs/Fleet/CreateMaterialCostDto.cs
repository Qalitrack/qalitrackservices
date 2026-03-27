namespace TechnicianApi.Core.DTOs.Fleet;

public class CreateMaterialCostDto
{
    public string MaterialId { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string? Location { get; set; }
}
