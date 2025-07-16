namespace SupplierService.Core.DTOs;

public class SupplierPerformanceDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public decimal Score { get; set; } // 0-100 scale
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int DataPoints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}