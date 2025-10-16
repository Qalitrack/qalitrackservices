using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities
{
    public class DriverVehicle : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string DriverId { get; set; } = null!;

        [ForeignKey(nameof(DriverId))]
        public virtual Driver Driver { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string VehicleId { get; set; } = null!;

        [ForeignKey(nameof(VehicleId))]
        public virtual Vehicle Vehicle { get; set; } = null!;

        // Optional tracking info
        public DateTime? AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } = "active";
    }
}