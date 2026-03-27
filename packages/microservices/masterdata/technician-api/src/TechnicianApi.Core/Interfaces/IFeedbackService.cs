using TechnicianApi.Core.DTOs.Feedback;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IFeedbackService
{
    Task<FeedbackResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<FeedbackResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? userId = null, string? status = null, string? feedbackType = null);
    Task<FeedbackResponseDto> CreateAsync(CreateFeedbackDto dto);
    Task<FeedbackResponseDto?> UpdateAsync(string id, UpdateFeedbackDto dto);
    Task<bool> DeleteAsync(string id);
    Task<FeedbackResponseDto?> RespondToFeedbackAsync(string id, RespondToFeedbackDto dto, string respondedBy);
    Task<FeedbackResponseDto?> ResolveAsync(string id);
    Task<FeedbackResponseDto?> RejectAsync(string id);
}
