using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicianApi.Core.Entities;

public class Trip : BaseEntity
{
    [Required]
    public string TruckId { get; set; } = string.Empty;

    [Required]
    public string DriverId { get; set; } = string.Empty;

    public string? TripTypeId { get; set; }

    [MaxLength(200)]
    public string? CustomTripType { get; set; }

    [Required]
    [MaxLength(500)]
    public string StartLocation { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string EndLocation { get; set; } = string.Empty;

    // Location coordinates (using separate lat/long instead of GIS PointField for simplicity)
    public double? StartLocationLatitude { get; set; }
    public double? StartLocationLongitude { get; set; }
    public double? EndLocationLatitude { get; set; }
    public double? EndLocationLongitude { get; set; }
    public double? TruckLocationLatitude { get; set; }
    public double? TruckLocationLongitude { get; set; }
    public double? CurrentLocationLatitude { get; set; }
    public double? CurrentLocationLongitude { get; set; }

    // Mileage
    public decimal? StartMileage { get; set; }
    public decimal? EndMileage { get; set; }
    public decimal? TotalMileage { get; set; }

    // Proof images
    public string? ProofImageUrl { get; set; }
    public string? ProofEndImageUrl { get; set; }

    // Material loading photos (stored as JSON array of URLs)
    public string? MaterialLoadingPhotosJson { get; set; }

    // Material info
    public string? MaterialId { get; set; }
    public string? MaterialVariantId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaterialCost { get; set; }

    public TripStatus Status { get; set; } = TripStatus.Pending;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; } = 0;

    // Navigation properties
    public virtual Truck Truck { get; set; } = null!;
    public virtual TripType? TripType { get; set; }
    public virtual Material? Material { get; set; }
    public virtual MaterialVariant? MaterialVariant { get; set; }
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public virtual ICollection<TripMaterial> TripMaterials { get; set; } = new List<TripMaterial>();
}

public enum TripStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}
