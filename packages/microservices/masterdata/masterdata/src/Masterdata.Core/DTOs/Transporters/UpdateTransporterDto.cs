using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Transporters;

public class UpdateTransporterDto
{
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string? Name { get; set; }
    
    public string? ContactInfo { get; set; }
    public string? Status { get; set; }
    public string? Logo { get; set; }
}
