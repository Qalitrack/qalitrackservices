using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.PerDiemReturn;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class PerDiemReturnFormService : IPerDiemReturnFormService
{
    private readonly IRepository<PerDiemReturnForm> _repository;
    private readonly IAssignmentBalanceService _balanceService;
    private readonly IMapper _mapper;

    public PerDiemReturnFormService(
        IRepository<PerDiemReturnForm> repository,
        IAssignmentBalanceService balanceService,
        IMapper mapper)
    {
        _repository = repository;
        _balanceService = balanceService;
        _mapper = mapper;
    }

    public async Task<PerDiemReturnFormResponseDto?> GetByIdAsync(string id)
    {
        var form = await _repository.GetByIdAsync(id);
        return form == null ? null : _mapper.Map<PerDiemReturnFormResponseDto>(form);
    }

    public async Task<PagedResponseDto<PerDiemReturnFormResponseDto>> GetByAssignmentIdAsync(
        string assignmentId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: f => f.AssignmentId == assignmentId,
            orderBy: f => f.CreatedAt,
            ascending: false
        );

        return new PagedResponseDto<PerDiemReturnFormResponseDto>
        {
            Items = _mapper.Map<IEnumerable<PerDiemReturnFormResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PerDiemReturnFormResponseDto> CreateAsync(CreatePerDiemReturnFormDto dto)
    {
        var form = _mapper.Map<PerDiemReturnForm>(dto);
        var created = await _repository.CreateAsync(form);
        return _mapper.Map<PerDiemReturnFormResponseDto>(created);
    }

    public async Task<PerDiemReturnFormResponseDto?> UpdateAsync(string id, UpdatePerDiemReturnFormDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);

        // Recalculate total if any expense field was updated
        if (dto.FaresOrCarExpense.HasValue || dto.Mileage.HasValue || dto.Meals.HasValue ||
            dto.Medical.HasValue || dto.Incidentals.HasValue)
        {
            existing.TotalAmount = existing.FaresOrCarExpense + existing.Mileage +
                                  existing.Meals + existing.Medical + existing.Incidentals;
        }

        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<PerDiemReturnFormResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<PerDiemReturnFormResponseDto?> ApproveAsync(string id, string approvedBy, string? comments = null)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != PerDiemReturnStatus.Pending)
            return null;

        form.Status = PerDiemReturnStatus.Approved;
        form.ApprovedAt = DateTime.UtcNow;
        form.ApprovedBy = approvedBy;
        form.ApprovalComments = comments;

        var updated = await _repository.UpdateAsync(form);
        if (updated != null)
        {
            // Recalculate balance after approval
            await _balanceService.RecalculateOnFormChangeAsync(form.AssignmentId);
        }

        return updated == null ? null : _mapper.Map<PerDiemReturnFormResponseDto>(updated);
    }

    public async Task<PerDiemReturnFormResponseDto?> RejectAsync(string id, string rejectedBy, string rejectionReason)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != PerDiemReturnStatus.Pending)
            return null;

        form.Status = PerDiemReturnStatus.Rejected;
        form.RejectedAt = DateTime.UtcNow;
        form.RejectedBy = rejectedBy;
        form.RejectionReason = rejectionReason;

        var updated = await _repository.UpdateAsync(form);
        return updated == null ? null : _mapper.Map<PerDiemReturnFormResponseDto>(updated);
    }
}
