using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Fleet;

public class MaterialPhotoResponseDto : BaseResponseDto
{
    public string MaterialId { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
}
