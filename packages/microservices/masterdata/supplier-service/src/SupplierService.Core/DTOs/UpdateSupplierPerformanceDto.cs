using System.ComponentModel.DataAnnotations;

namespace SupplierService.Core.DTOs;

public class UpdateSupplierPerformanceDto
{
    public string? MetricType { get; set; }

    [Range(0, 100)]
    public decimal? Score { get; set; }

    public string? Period { get; set; }

    public DateTime? PeriodStart { get; set; }

    public DateTime? PeriodEnd { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Range(1, int.MaxValue)]
    public int? DataPoints { get; set; }
}