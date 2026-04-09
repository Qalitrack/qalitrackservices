using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicianApi.Core.Entities;

public class VehicleMileage : BaseEntity
{
    [Required]
    public string TruckId { get; set; } = string.Empty;

    [Required]
    public string DriverId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal StartMileage { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EndMileage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Mileage { get; set; } // Calculated: EndMileage - StartMileage

    public string? ProofImageUrl { get; set; }
    public string? ProofEndImageUrl { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; } // References user service
    public bool Synced { get; set; } = false;

    // Navigation properties
    public virtual Truck Truck { get; set; } = null!;
}
