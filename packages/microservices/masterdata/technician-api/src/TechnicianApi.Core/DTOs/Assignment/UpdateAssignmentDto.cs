namespace TechnicianApi.Core.DTOs.Assignment;

public class UpdateAssignmentDto
{
    public List<string>? TechnicianIds { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ServiceType { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? LocationName { get; set; }
    public string? LocationAddress { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
}
