namespace TechnicianApi.Core.DTOs.Requisition;

public class CreateRequisitionDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string Type { get; set; } = "MaterialRequisition";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ItemsList { get; set; }
    public string? Justification { get; set; }
}
