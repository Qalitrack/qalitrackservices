using System.ComponentModel.DataAnnotations;

namespace SupplierService.Core.DTOs;

public class CreateSupplierPerformanceDto
{
    [Required]
    public string MetricType { get; set; } = "DeliveryTime";

    [Required]
    [Range(0, 100)]
    public decimal Score { get; set; } // 0-100 scale

    [Required]
    public string Period { get; set; } = "Month";

    [Required]
    public DateTime PeriodStart { get; set; }

    [Required]
    public DateTime PeriodEnd { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DataPoints { get; set; } = 1;
}