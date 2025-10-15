using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Vehicles;

public class CreateVehicleDto
{
    [Required]
    public string RegistrationNumber { get; set; } = null!;

    [Required]
    public string Type { get; set; } = null!;

    public string? Color { get; set; }
    public string? Model { get; set; }
    public string? Status { get; set; } = "active";
    public string? SupplierId { get; set; }
    public string? TransporterId { get; set; }
    public string? OwnerId { get; set; }
    public string? AxleConfigurationId { get; set; }
}
