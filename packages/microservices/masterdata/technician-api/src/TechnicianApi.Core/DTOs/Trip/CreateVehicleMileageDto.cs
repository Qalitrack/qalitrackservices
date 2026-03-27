using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.DTOs.Trip;

public class CreateVehicleMileageDto
{
    [Required]
    public string TruckId { get; set; } = string.Empty;

    [Required]
    public string DriverId { get; set; } = string.Empty;

    [Required]
    public decimal StartMileage { get; set; }

    [Required]
    public decimal EndMileage { get; set; }

    public string? ProofImageUrl { get; set; }
    public string? ProofEndImageUrl { get; set; }
    public DateTime? Date { get; set; }
    public string? UserId { get; set; }
}
