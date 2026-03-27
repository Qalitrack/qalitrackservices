namespace TechnicianApi.Core.DTOs.Feedback;

public class CreateFeedbackDto
{
    public string UserId { get; set; } = string.Empty;
    public string FeedbackType { get; set; } = string.Empty; // BugReport, FeatureRequest, General, Complaint, Suggestion
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
