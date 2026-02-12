using System;
using System.Collections.Generic;

namespace Masterdata.Core.DTOs.Drivers
{
    public class DriverReadDto
    {
        /// <summary>
        /// The unique identifier of the driver
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The full name of the driver
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// The email address of the driver
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// The phone number of the driver
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// The driver's license number
        /// </summary>
        public string LicenseNumber { get; set; } = null!;

        /// <summary>
        /// The date when the driver's license expires
        /// </summary>
        public DateTime? LicenseExpiryDate { get; set; }

        /// <summary>
        /// The current status of the driver (active, inactive, suspended, etc.)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// The ID of the transporter this driver is associated with
        /// </summary>
        public string? TransporterId { get; set; }

        /// <summary>
        /// The ID of the supplier this driver is associated with
        /// </summary>
        public string? SupplierId { get; set; }

        /// <summary>
        /// List of vehicle IDs assigned to this driver
        /// </summary>
        public IEnumerable<string> AssignedVehicleIds { get; set; } = new List<string>();

        /// <summary>
        /// The date and time when the driver record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the driver record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        public string? NfCcode { get; set; }

        /// <summary>
        /// Detailed information about vehicle assignments for this driver
        /// </summary>
        public IEnumerable<DriverVehicleReadDto>? VehicleAssignments { get; set; }
    }
}