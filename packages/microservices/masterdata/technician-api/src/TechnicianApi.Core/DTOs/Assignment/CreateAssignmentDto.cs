namespace TechnicianApi.Core.DTOs.Assignment;

public class CreateAssignmentDto
{
    public List<string> TechnicianIds { get; set; } = new List<string>();
    public string ManagerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public DateTime? Deadline { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
}
