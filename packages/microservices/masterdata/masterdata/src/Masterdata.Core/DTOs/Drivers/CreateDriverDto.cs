using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Masterdata.Core.DTOs.Drivers
{
    public class CreateDriverDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(200, ErrorMessage = "Full name cannot be longer than 200 characters")]
        public string FullName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Invalid phone number format (must be digits and may start with +)")]
        public string Phone { get; set; } = null!;

        [StringLength(50, ErrorMessage = "License number cannot be longer than 50 characters")]
        public string? LicenseNumber { get; set; }

        [StringLength(50, ErrorMessage = "ID number cannot be longer than 50 characters")]
        public string? IdNumber { get; set; }

        private DateTime? _licenseExpiryDate;

        /// <summary>
        /// The date when the driver's license expires. Must be in UTC format.
        /// Example: 2025-10-15T00:00:00Z
        /// </summary>
        [DataType(DataType.Date, ErrorMessage = "Invalid date format")]
        public DateTime? LicenseExpiryDate
        {
            get => _licenseExpiryDate;
            set
            {
                if (value.HasValue && value.Value < DateTime.UtcNow)
                    throw new ValidationException("License expiry date cannot be in the past.");

                _licenseExpiryDate = value?.ToUniversalTime();
            }
        }

        [StringLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
        public string? Status { get; set; } = "active";
    }
}
