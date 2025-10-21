using System.Text.Json;
using Masterdata.Core.DTOs.Affiliation;

namespace Masterdata.Core.DTOs.Organisation;

public class OrganisationDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public JsonDocument? ContactInfo { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<AffiliationDto> Affiliations { get; set; } = new List<AffiliationDto>();
}
