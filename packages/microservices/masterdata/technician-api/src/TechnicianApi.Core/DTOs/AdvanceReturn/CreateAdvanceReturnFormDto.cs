namespace TechnicianApi.Core.DTOs.AdvanceReturn;

public class CreateAdvanceReturnFormDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public List<AdvanceReturnLineItemDto> LineItems { get; set; } = new();
}
