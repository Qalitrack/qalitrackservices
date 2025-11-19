using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.Refund;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class RefundService : IRefundService
{
    private readonly IRepository<Refund> _repository;
    private readonly IAssignmentBalanceService _balanceService;
    private readonly IMapper _mapper;

    public RefundService(
        IRepository<Refund> repository,
        IAssignmentBalanceService balanceService,
        IMapper mapper)
    {
        _repository = repository;
        _balanceService = balanceService;
        _mapper = mapper;
    }

    public async Task<RefundResponseDto?> GetByIdAsync(string id)
    {
        var refund = await _repository.GetByIdAsync(id);
        return refund == null ? null : _mapper.Map<RefundResponseDto>(refund);
    }

    public async Task<PagedResponseDto<RefundResponseDto>> GetByAssignmentIdAsync(
        string assignmentId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: r => r.AssignmentId == assignmentId,
            orderBy: r => r.CreatedAt,
            ascending: false
        );

        return new PagedResponseDto<RefundResponseDto>
        {
            Items = _mapper.Map<IEnumerable<RefundResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RefundResponseDto> CreateAsync(CreateRefundDto dto)
    {
        var refund = _mapper.Map<Refund>(dto);
        var created = await _repository.CreateAsync(refund);
        return _mapper.Map<RefundResponseDto>(created);
    }

    public async Task<RefundResponseDto?> UpdateAsync(string id, UpdateRefundDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        if (existing.Status != RefundStatus.Pending)
            return null;

        _mapper.Map(dto, existing);
        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<RefundResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<RefundResponseDto?> ManagerApproveAsync(string id, ApproveRefundDto dto)
    {
        var refund = await _repository.GetByIdAsync(id);
        if (refund == null) return null;

        if (refund.Status != RefundStatus.Pending)
            return null;

        refund.Status = RefundStatus.ManagerApproved;
        refund.ManagerReviewedAt = DateTime.UtcNow;
        refund.ManagerReviewedBy = dto.ApprovedBy;
        refund.ManagerComments = dto.Comments;

        var updated = await _repository.UpdateAsync(refund);
        return updated == null ? null : _mapper.Map<RefundResponseDto>(updated);
    }

    public async Task<RefundResponseDto?> ManagerRejectAsync(string id, RejectRefundDto dto)
    {
        var refund = await _repository.GetByIdAsync(id);
        if (refund == null) return null;

        if (refund.Status != RefundStatus.Pending)
            return null;

        refund.Status = RefundStatus.ManagerRejected;
        refund.RejectedAt = DateTime.UtcNow;
        refund.RejectedBy = dto.RejectedBy;
        refund.RejectionReason = dto.RejectionReason;

        var updated = await _repository.UpdateAsync(refund);
        return updated == null ? null : _mapper.Map<RefundResponseDto>(updated);
    }

    public async Task<RefundResponseDto?> CfoConfirmReceivedAsync(string id, ConfirmRefundReceivedDto dto)
    {
        var refund = await _repository.GetByIdAsync(id);
        if (refund == null) return null;

        if (refund.Status != RefundStatus.ManagerApproved)
            return null;

        refund.Status = RefundStatus.CfoReceived;
        refund.CfoReviewedAt = DateTime.UtcNow;
        refund.CfoReviewedBy = dto.ReceivedBy;
        refund.CfoComments = dto.Comments;
        refund.ReceivedAt = DateTime.UtcNow;
        refund.ReceivedBy = dto.ReceivedBy;

        var updated = await _repository.UpdateAsync(refund);
        if (updated != null)
        {
            // Recalculate balance after CFO confirms receipt
            await _balanceService.RecalculateOnFormChangeAsync(refund.AssignmentId);
        }

        return updated == null ? null : _mapper.Map<RefundResponseDto>(updated);
    }

    public async Task<RefundResponseDto?> CfoRejectAsync(string id, RejectRefundDto dto)
    {
        var refund = await _repository.GetByIdAsync(id);
        if (refund == null) return null;

        if (refund.Status != RefundStatus.ManagerApproved)
            return null;

        refund.Status = RefundStatus.CfoRejected;
        refund.RejectedAt = DateTime.UtcNow;
        refund.RejectedBy = dto.RejectedBy;
        refund.RejectionReason = dto.RejectionReason;

        var updated = await _repository.UpdateAsync(refund);
        return updated == null ? null : _mapper.Map<RefundResponseDto>(updated);
    }
}
