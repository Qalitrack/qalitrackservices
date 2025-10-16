using System;
using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Drivers
{
    public class DriverVehicleReadDto
    {
        /// <summary>
        /// The unique identifier of the vehicle
        /// </summary>
        [Required]
        public string VehicleId { get; set; } = null!;

        /// <summary>
        /// The registration number of the vehicle
        /// </summary>
        [Required]
        public string RegistrationNumber { get; set; } = null!;

        /// <summary>
        /// The type of the vehicle
        /// </summary>
        [Required]
        public string VehicleType { get; set; } = null!;

        /// <summary>
        /// Indicates if this assignment is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The date and time when the driver was assigned to the vehicle
        /// </summary>
        [Required]
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// The date and time when the driver was unassigned from the vehicle (if applicable)
        /// </summary>
        public DateTime? UnassignedDate { get; set; }

        /// <summary>
        /// The status of the driver-vehicle assignment
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "active";
    }
}