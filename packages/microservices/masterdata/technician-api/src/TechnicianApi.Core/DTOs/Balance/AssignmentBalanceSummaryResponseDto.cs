namespace TechnicianApi.Core.DTOs.Balance;

public class AssignmentBalanceSummaryResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;

    // Money OUT (Company → Technician)
    public decimal TotalAdvancesGiven { get; set; }

    // Money IN/ACCOUNTED (Technician → Company)
    public decimal TotalReturns { get; set; }
    public decimal TotalExpensesClaimed { get; set; }
    public decimal TotalMaterialsRequisitioned { get; set; }
    public decimal TotalRefunds { get; set; }

    // Calculated totals
    public decimal TotalMoneyOut { get; set; }
    public decimal TotalMoneyAccountedFor { get; set; }

    // Net Balance
    public decimal NetBalance { get; set; }
    public string BalanceStatus { get; set; } = string.Empty;

    // Reconciliation
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciledBy { get; set; }
    public string? ReconciliationNotes { get; set; }

    // Approval for final closure
    public bool IsManagerApproved { get; set; }
    public DateTime? ManagerApprovedAt { get; set; }
    public string? ManagerApprovedBy { get; set; }

    public bool IsCfoApproved { get; set; }
    public DateTime? CfoApprovedAt { get; set; }
    public string? CfoApprovedBy { get; set; }

    // Last calculation timestamp
    public DateTime LastCalculatedAt { get; set; }

    // Intelligent Recommendations
    public string RecommendedAction { get; set; } = string.Empty;
    public decimal RecommendedAmount { get; set; }
    public string? RecommendationMessage { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
