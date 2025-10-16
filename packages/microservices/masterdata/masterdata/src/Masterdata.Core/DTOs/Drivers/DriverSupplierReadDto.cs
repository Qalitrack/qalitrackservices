using System;

namespace Masterdata.Core.DTOs.Drivers
{
    public class DriverSupplierReadDto
    {
        public string Id { get; set; } = null!;
        public string DriverId { get; set; } = null!;
        public string SupplierId { get; set; } = null!;
        public DateTime? AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
