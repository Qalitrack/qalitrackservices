using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Affiliation;

public class CreateAffiliationDto
{
    [Required(ErrorMessage = "Sacco ID is required")]
    public string SaccoId { get; set; } = null!;
    
    [Required(ErrorMessage = "Organisation ID is required")]
    public string OrganisationId { get; set; } = null!;
    
    [StringLength(50, ErrorMessage = "Type cannot be longer than 50 characters")]
    public string? Type { get; set; }
    
    [StringLength(500, ErrorMessage = "Details cannot be longer than 500 characters")]
    public string? Details { get; set; }
}
