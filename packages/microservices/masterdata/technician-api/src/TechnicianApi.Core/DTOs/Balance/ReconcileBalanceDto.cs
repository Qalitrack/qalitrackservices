namespace TechnicianApi.Core.DTOs.Balance;

public class ReconcileBalanceDto
{
    public string ReconciledBy { get; set; } = string.Empty;
    public string? ReconciliationNotes { get; set; }
}
