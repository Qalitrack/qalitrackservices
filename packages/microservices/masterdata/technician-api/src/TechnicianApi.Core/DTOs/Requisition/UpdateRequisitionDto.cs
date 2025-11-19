namespace TechnicianApi.Core.DTOs.Requisition;

public class UpdateRequisitionDto
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public string? ItemsList { get; set; }
    public string? Justification { get; set; }
    public string? Status { get; set; }
    public string? TmComments { get; set; }
    public string? CfoComments { get; set; }
    public string? VoucherNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? RejectionReason { get; set; }
}
