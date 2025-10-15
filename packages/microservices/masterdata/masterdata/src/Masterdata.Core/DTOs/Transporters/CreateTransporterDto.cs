using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Transporters;

public class CreateTransporterDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string Name { get; set; } = null!;

    public string? ContactInfo { get; set; }
    public string? Status { get; set; } = "active";
    public string? Logo { get; set; }
}
