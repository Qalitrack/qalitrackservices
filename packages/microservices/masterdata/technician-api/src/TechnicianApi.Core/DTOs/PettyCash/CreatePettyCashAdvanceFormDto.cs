namespace TechnicianApi.Core.DTOs.PettyCash;

public class CreatePettyCashAdvanceFormDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public decimal Sum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PreparedBy { get; set; } = string.Empty;
}
