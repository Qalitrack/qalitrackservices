using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class Feedback : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty; // References user service

    [Required]
    public FeedbackType FeedbackType { get; set; }

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;

    public string? AdminResponse { get; set; }

    public string? RespondedByUserId { get; set; } // References user service

    public DateTime? ResponseDate { get; set; }
}

public enum FeedbackType
{
    BugReport,
    FeatureRequest,
    General,
    Complaint,
    Suggestion
}

public enum FeedbackStatus
{
    Pending,
    Reviewed,
    InProgress,
    Resolved,
    Rejected
}
