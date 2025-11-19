namespace TechnicianApi.Core.DTOs.PerDiemReturn;

public class PerDiemReturnFormResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;

    // Expense breakdown
    public decimal FaresOrCarExpense { get; set; }
    public decimal Mileage { get; set; }
    public decimal Meals { get; set; }
    public decimal Medical { get; set; }
    public decimal Incidentals { get; set; }
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    // Approval details
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComments { get; set; }

    // Rejection details
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
