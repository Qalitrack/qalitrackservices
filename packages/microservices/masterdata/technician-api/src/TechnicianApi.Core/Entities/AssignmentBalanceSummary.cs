namespace TechnicianApi.Core.Entities;

public class AssignmentBalanceSummary : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;

    // Money OUT (Company → Technician)
    public decimal TotalAdvancesGiven { get; set; } // Petty Cash + Cash Advance Requisitions + Approved Claims

    // Money IN/ACCOUNTED (Technician → Company)
    public decimal TotalReturns { get; set; } // Advance Returns
    public decimal TotalExpensesClaimed { get; set; } // Per Diem Returns
    public decimal TotalMaterialsRequisitioned { get; set; } // Material Requisitions
    public decimal TotalRefunds { get; set; } // Direct refunds from technician

    // Calculated totals
    public decimal TotalMoneyOut { get; set; } // Sum of all advances
    public decimal TotalMoneyAccountedFor { get; set; } // Returns + Expenses + Materials + Refunds

    // Net Balance
    public decimal NetBalance { get; set; } // TotalMoneyOut - TotalMoneyAccountedFor
    public BalanceStatus BalanceStatus { get; set; } // Who owes whom

    // Reconciliation
    public bool IsReconciled { get; set; } = false;
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciledBy { get; set; }
    public string? ReconciliationNotes { get; set; }

    // Approval for final closure
    public bool IsManagerApproved { get; set; } = false;
    public DateTime? ManagerApprovedAt { get; set; }
    public string? ManagerApprovedBy { get; set; }

    public bool IsCfoApproved { get; set; } = false;
    public DateTime? CfoApprovedAt { get; set; }
    public string? CfoApprovedBy { get; set; }

    // Last calculation timestamp
    public DateTime LastCalculatedAt { get; set; } = DateTime.UtcNow;

    // Intelligent Recommendations
    public RecommendedAction RecommendedAction { get; set; } = RecommendedAction.None;
    public decimal RecommendedAmount { get; set; } = 0;
    public string? RecommendationMessage { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
}

public enum BalanceStatus
{
    Balanced,           // NetBalance = 0 (all settled)
    TechnicianOwes,     // NetBalance > 0 (technician needs to return money)
    CompanyOwes         // NetBalance < 0 (company needs to reimburse technician)
}

public enum RecommendedAction
{
    None,               // Balance is settled, no action needed
    CreateRefund,       // Technician should create a refund (owes company)
    CreateClaim         // Technician should create a claim (company owes technician)
}
