using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Drivers
{
    public class UpdateDriverDto
    {
        [StringLength(200, ErrorMessage = "Full name cannot be longer than 200 characters")]
        public string? FullName { get; set; }

        [StringLength(50, ErrorMessage = "License number cannot be longer than 50 characters")]
        public string? LicenseNumber { get; set; }

        private DateTime? _licenseExpiryDate;

        [DataType(DataType.Date)]
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

        [RegularExpression(@"^(active|inactive|suspended)$", ErrorMessage = "Status must be active, inactive, or suspended")]
        public string? Status { get; set; }
    }
}