using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.PettyCash;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class PettyCashAdvanceFormService : IPettyCashAdvanceFormService
{
    private readonly IRepository<PettyCashAdvanceForm> _repository;
    private readonly IAssignmentBalanceService _balanceService;
    private readonly IMapper _mapper;

    public PettyCashAdvanceFormService(
        IRepository<PettyCashAdvanceForm> repository,
        IAssignmentBalanceService balanceService,
        IMapper mapper)
    {
        _repository = repository;
        _balanceService = balanceService;
        _mapper = mapper;
    }

    public async Task<PettyCashAdvanceFormResponseDto?> GetByIdAsync(string id)
    {
        var form = await _repository.GetByIdAsync(id);
        return form == null ? null : _mapper.Map<PettyCashAdvanceFormResponseDto>(form);
    }

    public async Task<PagedResponseDto<PettyCashAdvanceFormResponseDto>> GetByAssignmentIdAsync(
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

        return new PagedResponseDto<PettyCashAdvanceFormResponseDto>
        {
            Items = _mapper.Map<IEnumerable<PettyCashAdvanceFormResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PettyCashAdvanceFormResponseDto> CreateAsync(CreatePettyCashAdvanceFormDto dto)
    {
        var form = _mapper.Map<PettyCashAdvanceForm>(dto);
        var created = await _repository.CreateAsync(form);
        return _mapper.Map<PettyCashAdvanceFormResponseDto>(created);
    }

    public async Task<PettyCashAdvanceFormResponseDto?> UpdateAsync(string id, UpdatePettyCashAdvanceFormDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);
        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<PettyCashAdvanceFormResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<PettyCashAdvanceFormResponseDto?> ApproveAsync(string id, string approvedBy, string? comments = null)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != PettyCashStatus.Pending)
            return null;

        form.Status = PettyCashStatus.Approved;
        form.ApprovedAt = DateTime.UtcNow;
        form.ApprovedBy = approvedBy;
        form.ApprovalComments = comments;

        var updated = await _repository.UpdateAsync(form);
        return updated == null ? null : _mapper.Map<PettyCashAdvanceFormResponseDto>(updated);
    }

    public async Task<PettyCashAdvanceFormResponseDto?> RejectAsync(string id, string rejectedBy, string rejectionReason)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != PettyCashStatus.Pending)
            return null;

        form.Status = PettyCashStatus.Rejected;
        form.RejectedAt = DateTime.UtcNow;
        form.RejectedBy = rejectedBy;
        form.RejectionReason = rejectionReason;

        var updated = await _repository.UpdateAsync(form);
        return updated == null ? null : _mapper.Map<PettyCashAdvanceFormResponseDto>(updated);
    }

    public async Task<PettyCashAdvanceFormResponseDto?> DisburseAsync(
        string id,
        string disbursedBy,
        string voucherNumber,
        string? referenceNumber = null)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != PettyCashStatus.Approved)
            return null;

        form.Status = PettyCashStatus.Disbursed;
        form.DisbursedAt = DateTime.UtcNow;
        form.DisbursedBy = disbursedBy;
        form.VoucherNumber = voucherNumber;
        form.ReferenceNumber = referenceNumber;

        var updated = await _repository.UpdateAsync(form);
        if (updated != null)
        {
            // Recalculate balance after disbursement
            await _balanceService.RecalculateOnFormChangeAsync(form.AssignmentId);
        }

        return updated == null ? null : _mapper.Map<PettyCashAdvanceFormResponseDto>(updated);
    }
}
