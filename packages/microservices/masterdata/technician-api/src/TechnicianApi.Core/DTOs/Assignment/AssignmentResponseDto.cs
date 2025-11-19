using TechnicianApi.Core.DTOs.Base;

using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Assignment;

public class AssignmentResponseDto : BaseResponseDto
{
    public List<string> TechnicianIds { get; set; } = new();
    public string ManagerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
