using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class AxleConfiguration : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = null!; // e.g., "6x4", "8x2"

    [MaxLength(200)]
    public string? Description { get; set; } // e.g., "Three axles, two driven"

    public int? AxleCount { get; set; } // optional but useful

    public decimal? MaxLoadCapacity { get; set; } // e.g., total weight in tons

    public bool IsActive { get; set; } = true;

    // One-to-Many: One axle configuration → many vehicles
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}