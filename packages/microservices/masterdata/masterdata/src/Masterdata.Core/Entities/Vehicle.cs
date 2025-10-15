using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Vehicle : BaseEntity
{
    [Required]
    public string RegistrationNumber { get; set; } = null!;

    [Required]
    public string Type { get; set; } = null!;

    public string? Color { get; set; }

    public string? Model { get; set; }

    public string? Status { get; set; } = "active";

    public string? SupplierId { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    public string? TransporterId { get; set; }

    [ForeignKey(nameof(TransporterId))]
    public virtual Transporter? Transporter { get; set; }

    public string? DriverId { get; set; }

    [ForeignKey(nameof(DriverId))]
    public virtual Driver? Driver { get; set; }

    public string? AxleConfigurationId { get; set; }

    [ForeignKey(nameof(AxleConfigurationId))]
    public virtual AxleConfiguration? AxleConfiguration { get; set; }

    public string? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual Owner? Owner { get; set; }

    public virtual ICollection<Driver> AssignedDrivers { get; set; } = new List<Driver>();
}
