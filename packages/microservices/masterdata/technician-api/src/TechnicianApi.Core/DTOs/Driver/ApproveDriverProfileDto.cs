namespace TechnicianApi.Core.DTOs.Driver;

public class ApproveDriverProfileDto
{
    public string Action { get; set; } = string.Empty; // approve, reject, changes_requested
    public string? Notes { get; set; }
}
