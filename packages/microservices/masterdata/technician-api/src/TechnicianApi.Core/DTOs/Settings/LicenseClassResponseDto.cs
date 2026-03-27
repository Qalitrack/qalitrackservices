using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Settings;

public class LicenseClassResponseDto : BaseResponseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
