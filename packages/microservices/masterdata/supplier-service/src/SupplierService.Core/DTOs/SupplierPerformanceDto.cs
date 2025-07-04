namespace SupplierService.Core.DTOs;

public class SupplierPerformanceDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? QualityRating { get; set; }
    public decimal? DeliveryRating { get; set; }
    public decimal? ServiceRating { get; set; }
    public decimal? OverallRating { get; set; }
    public int TotalOrders { get; set; }
    public int OnTimeDeliveries { get; set; }
    public int LateDeliveries { get; set; }
    public int DefectiveDeliveries { get; set; }
    public decimal OnTimeDeliveryRate { get; set; }
    public decimal DefectRate { get; set; }
    public decimal? AverageLeadTime { get; set; }
    public decimal? TotalOrderValue { get; set; }
    public string? Currency { get; set; }
    public int ComplaintCount { get; set; }
    public int ResolvedComplaints { get; set; }
    public decimal? CustomerSatisfactionScore { get; set; }
    public string? Comments { get; set; }
    public DateTime EvaluationDate { get; set; }
    public string? EvaluatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSupplierPerformanceRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? QualityRating { get; set; }
    public decimal? DeliveryRating { get; set; }
    public decimal? ServiceRating { get; set; }
    public decimal? OverallRating { get; set; }
    public int TotalOrders { get; set; } = 0;
    public int OnTimeDeliveries { get; set; } = 0;
    public int LateDeliveries { get; set; } = 0;
    public int DefectiveDeliveries { get; set; } = 0;
    public decimal? AverageLeadTime { get; set; }
    public decimal? TotalOrderValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public int ComplaintCount { get; set; } = 0;
    public int ResolvedComplaints { get; set; } = 0;
    public decimal? CustomerSatisfactionScore { get; set; }
    public string? Comments { get; set; }
    public string? EvaluatedBy { get; set; }
}