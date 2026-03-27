using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class Driver : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty; // References user service

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    // Navigation properties
    public virtual ICollection<DriverProfile> ProfileVersions { get; set; } = new List<DriverProfile>();
    public virtual ICollection<DriverActivity> Activities { get; set; } = new List<DriverActivity>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<VehicleMileage> VehicleMileages { get; set; } = new List<VehicleMileage>();
}
