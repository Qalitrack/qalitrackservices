using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Trip;

public class TripTypeResponseDto : BaseResponseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; } = string.Empty;
    public string EmptyTripOption { get; set; } = string.Empty;
    public string MaterialRequirement { get; set; } = string.Empty;
    public string? CreatedByUserId { get; set; }
}
