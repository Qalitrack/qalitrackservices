namespace TechnicianApi.Core.DTOs.Feedback;

public class UpdateFeedbackDto
{
    public string FeedbackType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
