using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Fleet;

public class MaterialVariantPhotoResponseDto : BaseResponseDto
{
    public string MaterialVariantId { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
}
