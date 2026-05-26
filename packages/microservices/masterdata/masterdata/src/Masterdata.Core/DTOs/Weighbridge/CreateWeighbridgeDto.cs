using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Weighbridge;

public class CreateWeighbridgeDto
{
    [Required(ErrorMessage = "Location is required")]
    [StringLength(200, ErrorMessage = "Location cannot be longer than 200 characters")]
    public string Location { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
    public string? Status { get; set; } = "active";

    public List<string> Scales { get; set; } = [];
}
