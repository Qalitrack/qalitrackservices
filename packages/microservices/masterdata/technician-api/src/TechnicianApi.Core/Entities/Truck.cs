using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class Truck : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string LicensePlate { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Model { get; set; }

    public string? DriverId { get; set; }

    // Navigation properties
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<VehicleMileage> VehicleMileages { get; set; } = new List<VehicleMileage>();
}
