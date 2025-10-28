using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Route;

public class CreateRouteDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Start point is required")]
    [StringLength(200, ErrorMessage = "Start point cannot be longer than 200 characters")]
    public string StartPoint { get; set; } = null!;

    [Required(ErrorMessage = "End point is required")]
    [StringLength(200, ErrorMessage = "End point cannot be longer than 200 characters")]
    public string EndPoint { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
    public string? Status { get; set; } = "active";
}
