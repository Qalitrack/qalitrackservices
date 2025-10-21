using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Masterdata.Core.DTOs.Organisation;

public class UpdateOrganisationDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string Name { get; set; } = null!;

    public JsonDocument? ContactInfo { get; set; }
    
    [StringLength(50, ErrorMessage = "Type cannot be longer than 50 characters")]
    public string? Type { get; set; }

    [StringLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
    public string? Status { get; set; }
}
