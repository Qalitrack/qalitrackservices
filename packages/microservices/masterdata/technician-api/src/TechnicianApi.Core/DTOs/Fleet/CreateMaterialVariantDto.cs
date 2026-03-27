namespace TechnicianApi.Core.DTOs.Fleet;

public class CreateMaterialVariantDto
{
    public string MaterialId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
