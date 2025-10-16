using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities
{
    public class Driver : BaseEntity
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, Phone, MaxLength(20)]
        public string Phone { get; set; } = null!;

        [Required, MaxLength(50)]
        public string LicenseNumber { get; set; } = null!;

        public DateTime? LicenseExpiryDate { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } = "active";

        public string? TransporterId { get; set; }

        [ForeignKey(nameof(TransporterId))]
        public virtual Transporter? Transporter { get; set; }

        public string? SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }

        /// <summary>
        /// Navigation property for the join table representing vehicle assignments
        /// </summary>
        public virtual ICollection<DriverVehicle> DriverVehicles { get; set; } = new List<DriverVehicle>();
    }
}