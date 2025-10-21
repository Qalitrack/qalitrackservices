namespace Masterdata.Core.DTOs.Affiliation;

public class AffiliationDto
{
    public string Id { get; set; } = null!;
    public string? SaccoId { get; set; }
    public string? SaccoName { get; set; }  // Will be mapped from Sacco
    public string? OrganisationId { get; set; }
    public string? OrganisationName { get; set; }  // Will be mapped from Organisation
    public string? Type { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
