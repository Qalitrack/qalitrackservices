using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Supplier
{
    public class CreateSupplierDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }
    }
}
