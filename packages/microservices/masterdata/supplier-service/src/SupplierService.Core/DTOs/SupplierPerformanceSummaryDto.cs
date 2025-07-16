namespace SupplierService.Core.DTOs;

public class SupplierPerformanceSummaryDto
{
    public string SupplierId { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal OverallScore { get; set; }
    public decimal DeliveryTimeScore { get; set; }
    public decimal QualityScore { get; set; }
    public decimal ResponseTimeScore { get; set; }
    public decimal ReliabilityScore { get; set; }
    public decimal PriceCompetitivenessScore { get; set; }
    public decimal CommunicationScore { get; set; }
    public int TotalDataPoints { get; set; }
    public DateTime LastUpdated { get; set; }
    public string PerformanceGrade { get; set; } = string.Empty; // A, B, C, D, F
    public List<SupplierPerformanceDto> RecentMetrics { get; set; } = new();
}