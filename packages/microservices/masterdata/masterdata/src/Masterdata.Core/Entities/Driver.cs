using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Driver : BaseEntity
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string LicenseNumber { get; set; } = null!;

    public DateTime? LicenseExpiryDate { get; set; }

    public string? Status { get; set; } = "active";

    public string? TransporterId { get; set; }

    [ForeignKey(nameof(TransporterId))]
    public virtual Transporter? Transporter { get; set; }

    public string? SupplierId { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<Vehicle> AssignedVehicles { get; set; } = new List<Vehicle>();
}
