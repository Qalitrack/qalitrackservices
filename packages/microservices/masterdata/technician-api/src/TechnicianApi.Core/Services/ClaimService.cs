using AutoMapper;
using Microsoft.AspNetCore.Http;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.Claim;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.DTOs.Attachment;

namespace TechnicianApi.Core.Services;

public class ClaimService : IClaimService
{
    private readonly IRepository<Claim> _repository;
    private readonly IAssignmentBalanceService _balanceService;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimService(
        IRepository<Claim> repository,
        IAssignmentBalanceService balanceService,
        IFileStorageService fileStorage,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _balanceService = balanceService;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ClaimResponseDto?> GetByIdAsync(string id)
    {
        var claim = await _repository.GetByIdAsync(id);
        if (claim == null) return null;
        
        var dto = _mapper.Map<ClaimResponseDto>(claim);
        await IncludeAttachments(dto);
        return dto;
    }

    public async Task<PagedResponseDto<ClaimResponseDto>> GetByAssignmentIdAsync(
        string assignmentId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: c => c.AssignmentId == assignmentId,
            orderBy: c => c.CreatedAt,
            ascending: false
        );

        return new PagedResponseDto<ClaimResponseDto>
        {
            Items = _mapper.Map<IEnumerable<ClaimResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task IncludeAttachments(ClaimResponseDto dto)
    {
        var attachments = await _fileStorage.GetAttachmentsForEntityAsync(nameof(Claim), dto.Id);
        dto.Attachments = _mapper.Map<ICollection<AttachmentDto>>(attachments).ToList();
        
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            foreach (var attachment in dto.Attachments)
            {
                attachment.FileUrl = $"{request.Scheme}://{request.Host}/api/claims/attachments/{attachment.Id}";
            }
        }
    }

    public async Task<ClaimResponseDto> CreateAsync(CreateClaimDto dto)
    {
        var claim = _mapper.Map<Claim>(dto);
        var created = await _repository.CreateAsync(claim);
        return _mapper.Map<ClaimResponseDto>(created);
    }

    public async Task<ClaimResponseDto?> UpdateAsync(string id, UpdateClaimDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        if (existing.Status != ClaimStatus.Pending)
            return null;

        _mapper.Map(dto, existing);
        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<ClaimResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<ClaimResponseDto?> ManagerApproveAsync(string id, ApproveClaimDto dto)
    {
        var claim = await _repository.GetByIdAsync(id);
        if (claim == null) return null;

        if (claim.Status != ClaimStatus.Pending)
            return null;

        claim.Status = ClaimStatus.ManagerApproved;
        claim.ManagerReviewedAt = DateTime.UtcNow;
        claim.ManagerReviewedBy = dto.ApprovedBy;
        claim.ManagerComments = dto.Comments;

        var updated = await _repository.UpdateAsync(claim);
        return updated == null ? null : _mapper.Map<ClaimResponseDto>(updated);
    }

    public async Task<ClaimResponseDto?> ManagerRejectAsync(string id, RejectClaimDto dto)
    {
        var claim = await _repository.GetByIdAsync(id);
        if (claim == null) return null;

        if (claim.Status != ClaimStatus.Pending)
            return null;

        claim.Status = ClaimStatus.ManagerRejected;
        claim.RejectedAt = DateTime.UtcNow;
        claim.RejectedBy = dto.RejectedBy;
        claim.RejectionReason = dto.RejectionReason;

        var updated = await _repository.UpdateAsync(claim);
        return updated == null ? null : _mapper.Map<ClaimResponseDto>(updated);
    }

    public async Task<ClaimResponseDto?> CfoApproveAsync(string id, ApproveClaimDto dto)
    {
        var claim = await _repository.GetByIdAsync(id);
        if (claim == null) return null;

        if (claim.Status != ClaimStatus.ManagerApproved)
            return null;

        claim.Status = ClaimStatus.CfoApproved;
        claim.CfoReviewedAt = DateTime.UtcNow;
        claim.CfoReviewedBy = dto.ApprovedBy;
        claim.CfoComments = dto.Comments;

        var updated = await _repository.UpdateAsync(claim);
        return updated == null ? null : _mapper.Map<ClaimResponseDto>(updated);
    }

    public async Task<ClaimResponseDto?> CfoRejectAsync(string id, RejectClaimDto dto)
    {
        var claim = await _repository.GetByIdAsync(id);
        if (claim == null) return null;

        if (claim.Status != ClaimStatus.ManagerApproved)
            return null;

        claim.Status = ClaimStatus.CfoRejected;
        claim.RejectedAt = DateTime.UtcNow;
        claim.RejectedBy = dto.RejectedBy;
        claim.RejectionReason = dto.RejectionReason;

        var updated = await _repository.UpdateAsync(claim);
        return updated == null ? null : _mapper.Map<ClaimResponseDto>(updated);
    }

    public async Task<ClaimResponseDto?> DisburseAsync(string id, DisburseClaimDto dto)
    {
        var claim = await _repository.GetByIdAsync(id);
        if (claim == null) return null;

        if (claim.Status != ClaimStatus.CfoApproved)
            return null;

        claim.Status = ClaimStatus.Disbursed;
        claim.DisbursedAt = DateTime.UtcNow;
        claim.DisbursedBy = dto.DisbursedBy;
        claim.VoucherNumber = dto.VoucherNumber;
        claim.ReferenceNumber = dto.ReferenceNumber;
        claim.PaymentMethod = dto.PaymentMethod;

        var updated = await _repository.UpdateAsync(claim);
        if (updated != null)
        {
            // Recalculate balance after disbursement
            await _balanceService.RecalculateOnFormChangeAsync(claim.AssignmentId);
        }

        return updated == null ? null : _mapper.Map<ClaimResponseDto>(updated);
    }
}
