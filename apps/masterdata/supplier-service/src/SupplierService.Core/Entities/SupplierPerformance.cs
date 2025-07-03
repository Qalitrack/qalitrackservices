namespace SupplierService.Core.Entities;

public class SupplierPerformance : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? QualityRating { get; set; } // 0-10 scale
    public decimal? DeliveryRating { get; set; } // 0-10 scale
    public decimal? ServiceRating { get; set; } // 0-10 scale
    public decimal? OverallRating { get; set; } // 0-10 scale
    public int TotalOrders { get; set; } = 0;
    public int OnTimeDeliveries { get; set; } = 0;
    public int LateDeliveries { get; set; } = 0;
    public int DefectiveDeliveries { get; set; } = 0;
    public decimal OnTimeDeliveryRate { get; set; } = 0;
    public decimal DefectRate { get; set; } = 0;
    public decimal? AverageLeadTime { get; set; }
    public decimal? TotalOrderValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public int ComplaintCount { get; set; } = 0;
    public int ResolvedComplaints { get; set; } = 0;
    public decimal? CustomerSatisfactionScore { get; set; }
    public string? Comments { get; set; }
    public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;
    public string? EvaluatedBy { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
}