namespace TechnicianApi.Core.DTOs.Feedback;

public class RespondToFeedbackDto
{
    public string AdminResponse { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Reviewed, InProgress, Resolved, Rejected
}
