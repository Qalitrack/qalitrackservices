using AutoMapper;
using TechnicianApi.Core.DTOs.Feedback;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IRepository<Feedback> _repository;
    private readonly IMapper _mapper;

    public FeedbackService(IRepository<Feedback> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<FeedbackResponseDto?> GetByIdAsync(string id)
    {
        var feedback = await _repository.GetByIdAsync(id);
        return feedback == null ? null : _mapper.Map<FeedbackResponseDto>(feedback);
    }

    public async Task<PagedResponseDto<FeedbackResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? userId = null, string? status = null, string? feedbackType = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: f =>
                (userId == null || f.UserId == userId) &&
                (status == null || f.Status.ToString() == status) &&
                (feedbackType == null || f.FeedbackType.ToString() == feedbackType)
        );

        var dtos = _mapper.Map<IEnumerable<FeedbackResponseDto>>(items);

        return new PagedResponseDto<FeedbackResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<FeedbackResponseDto> CreateAsync(CreateFeedbackDto dto)
    {
        var feedback = _mapper.Map<Feedback>(dto);
        var created = await _repository.CreateAsync(feedback);
        return _mapper.Map<FeedbackResponseDto>(created);
    }

    public async Task<FeedbackResponseDto?> UpdateAsync(string id, UpdateFeedbackDto dto)
    {
        var feedback = await _repository.GetByIdAsync(id);
        if (feedback == null) return null;

        _mapper.Map(dto, feedback);
        var updated = await _repository.UpdateAsync(feedback);
        return _mapper.Map<FeedbackResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<FeedbackResponseDto?> RespondToFeedbackAsync(
        string id, RespondToFeedbackDto dto, string respondedBy)
    {
        var feedback = await _repository.GetByIdAsync(id);
        if (feedback == null) return null;

        feedback.AdminResponse = dto.AdminResponse;
        feedback.Status = Enum.Parse<FeedbackStatus>(dto.Status);
        feedback.RespondedByUserId = respondedBy;
        feedback.ResponseDate = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(feedback);
        return _mapper.Map<FeedbackResponseDto>(updated);
    }

    public async Task<FeedbackResponseDto?> ResolveAsync(string id)
    {
        var feedback = await _repository.GetByIdAsync(id);
        if (feedback == null) return null;

        feedback.Status = FeedbackStatus.Resolved;
        var updated = await _repository.UpdateAsync(feedback);
        return _mapper.Map<FeedbackResponseDto>(updated);
    }

    public async Task<FeedbackResponseDto?> RejectAsync(string id)
    {
        var feedback = await _repository.GetByIdAsync(id);
        if (feedback == null) return null;

        feedback.Status = FeedbackStatus.Rejected;
        var updated = await _repository.UpdateAsync(feedback);
        return _mapper.Map<FeedbackResponseDto>(updated);
    }
}
