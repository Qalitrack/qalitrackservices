using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.DTOs.Trip;

public class CreateTripTypeDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public string Category { get; set; } = "Other";

    public string EmptyTripOption { get; set; } = "NotAllowed";

    public string MaterialRequirement { get; set; } = "None";

    public string? CreatedByUserId { get; set; }
}
