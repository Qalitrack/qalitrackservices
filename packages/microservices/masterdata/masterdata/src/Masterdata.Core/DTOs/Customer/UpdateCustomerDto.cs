using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Masterdata.Core.DTOs.Customer;

public class UpdateCustomerDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string Name { get; set; } = null!;

    public JsonDocument? ContactInfo { get; set; }

    [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
    public string? Status { get; set; }

    [StringLength(500, ErrorMessage = "Logo URL cannot be longer than 500 characters")]
    [Url(ErrorMessage = "Logo must be a valid URL")]
    public string? Logo { get; set; }
}
