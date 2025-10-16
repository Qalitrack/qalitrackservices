using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities
{
    public class Vehicle : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;

        [MaxLength(50)]
        public string? Model { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        // Supplier relationship
        public string? SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }

        // Transporter relationship
        public string? TransporterId { get; set; }

        [ForeignKey(nameof(TransporterId))]
        public virtual Transporter? Transporter { get; set; }

        // AxleConfiguration relationship
        [Required]
        public string AxleConfigurationId { get; set; } = null!;

        [ForeignKey(nameof(AxleConfigurationId))]
        public virtual AxleConfiguration AxleConfiguration { get; set; } = null!;

        // Owner relationship
        [Required]
        public string OwnerId { get; set; } = null!;

        [ForeignKey(nameof(OwnerId))]
        public virtual Owner Owner { get; set; } = null!;

        /// <summary>
        /// Navigation property for the join table representing driver assignments
        /// </summary>
        public virtual ICollection<DriverVehicle> DriverVehicles { get; set; } = new List<DriverVehicle>();
    }
}