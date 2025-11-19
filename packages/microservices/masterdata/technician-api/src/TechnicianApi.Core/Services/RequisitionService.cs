using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.Requisition;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class RequisitionService : IRequisitionService
{
    private readonly IRepository<Requisition> _repository;
    private readonly IMapper _mapper;

    public RequisitionService(IRepository<Requisition> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RequisitionResponseDto?> GetByIdAsync(string id)
    {
        var requisition = await _repository.GetByIdAsync(id);
        return requisition == null ? null : _mapper.Map<RequisitionResponseDto>(requisition);
    }

    public async Task<IEnumerable<RequisitionResponseDto>> GetByAssignmentIdAsync(string assignmentId)
    {
        var requisitions = await _repository.FindAsync(r => r.AssignmentId == assignmentId);
        return _mapper.Map<IEnumerable<RequisitionResponseDto>>(requisitions);
    }

    public async Task<PagedResponseDto<RequisitionResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: req =>
                (string.IsNullOrEmpty(technicianId) || req.TechnicianId == technicianId) &&
                (string.IsNullOrEmpty(status) || req.Status.ToString() == status),
            orderBy: r => r.CreatedAt,
            ascending: false
        );

        return new PagedResponseDto<RequisitionResponseDto>
        {
            Items = _mapper.Map<IEnumerable<RequisitionResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RequisitionResponseDto> CreateAsync(CreateRequisitionDto dto)
    {
        var requisition = _mapper.Map<Requisition>(dto);
        var created = await _repository.CreateAsync(requisition);
        return _mapper.Map<RequisitionResponseDto>(created);
    }

    public async Task<RequisitionResponseDto?> UpdateAsync(string id, UpdateRequisitionDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);
        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<RequisitionResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<RequisitionResponseDto?> TmApproveAsync(string id, string tmId, string? comments)
    {
        var requisition = await _repository.GetByIdAsync(id);
        if (requisition == null) return null;

        if (requisition.Status != RequisitionStatus.Pending)
            return null;

        requisition.Status = RequisitionStatus.TmApproved;
        requisition.TmReviewedAt = DateTime.UtcNow;
        requisition.TmReviewedBy = tmId;
        requisition.TmComments = comments;

        var updated = await _repository.UpdateAsync(requisition);
        return updated == null ? null : _mapper.Map<RequisitionResponseDto>(updated);
    }

    public async Task<RequisitionResponseDto?> TmRejectAsync(string id, string tmId, string rejectionReason)
    {
        var requisition = await _repository.GetByIdAsync(id);
        if (requisition == null) return null;

        if (requisition.Status != RequisitionStatus.Pending)
            return null;

        requisition.Status = RequisitionStatus.TmRejected;
        requisition.TmReviewedAt = DateTime.UtcNow;
        requisition.TmReviewedBy = tmId;
        requisition.RejectionReason = rejectionReason;
        requisition.RejectedAt = DateTime.UtcNow;
        requisition.RejectedBy = tmId;

        var updated = await _repository.UpdateAsync(requisition);
        return updated == null ? null : _mapper.Map<RequisitionResponseDto>(updated);
    }

    public async Task<RequisitionResponseDto?> CfoApproveAsync(string id, string cfoId, string? comments)
    {
        var requisition = await _repository.GetByIdAsync(id);
        if (requisition == null) return null;

        if (requisition.Status != RequisitionStatus.TmApproved && requisition.Status != RequisitionStatus.CfoProcessing)
            return null;

        requisition.Status = RequisitionStatus.CfoApproved;
        requisition.CfoReviewedAt = DateTime.UtcNow;
        requisition.CfoReviewedBy = cfoId;
        requisition.CfoComments = comments;

        var updated = await _repository.UpdateAsync(requisition);
        return updated == null ? null : _mapper.Map<RequisitionResponseDto>(updated);
    }

    public async Task<RequisitionResponseDto?> CfoRejectAsync(string id, string cfoId, string rejectionReason)
    {
        var requisition = await _repository.GetByIdAsync(id);
        if (requisition == null) return null;

        if (requisition.Status != RequisitionStatus.TmApproved && requisition.Status != RequisitionStatus.CfoProcessing)
            return null;

        requisition.Status = RequisitionStatus.CfoRejected;
        requisition.CfoReviewedAt = DateTime.UtcNow;
        requisition.CfoReviewedBy = cfoId;
        requisition.RejectionReason = rejectionReason;
        requisition.RejectedAt = DateTime.UtcNow;
        requisition.RejectedBy = cfoId;

        var updated = await _repository.UpdateAsync(requisition);
        return updated == null ? null : _mapper.Map<RequisitionResponseDto>(updated);
    }

    public async Task<RequisitionResponseDto?> MarkAsPaidAsync(string id, string voucherNumber, string referenceNumber)
    {
        var requisition = await _repository.GetByIdAsync(id);
        if (requisition == null) return null;

        if (requisition.Status != RequisitionStatus.CfoApproved)
            return null;

        requisition.Status = RequisitionStatus.Paid;
        requisition.VoucherNumber = voucherNumber;
        requisition.ReferenceNumber = referenceNumber;
        requisition.PaidAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(requisition);
        return updated == null ? null : _mapper.Map<RequisitionResponseDto>(updated);
    }
}
