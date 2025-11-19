using AutoMapper;
using Microsoft.AspNetCore.Http;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.AdvanceReturn;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.DTOs.Attachment;

namespace TechnicianApi.Core.Services;

public class AdvanceReturnFormService : IAdvanceReturnFormService
{
    private readonly IRepository<AdvanceReturnForm> _repository;
    private readonly IRepository<AdvanceReturnLineItem> _lineItemRepository;
    private readonly IAssignmentBalanceService _balanceService;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdvanceReturnFormService(
        IRepository<AdvanceReturnForm> repository,
        IRepository<AdvanceReturnLineItem> lineItemRepository,
        IAssignmentBalanceService balanceService,
        IFileStorageService fileStorage,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _lineItemRepository = lineItemRepository;
        _balanceService = balanceService;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AdvanceReturnFormResponseDto?> GetByIdAsync(string id)
    {
        var form = await _repository.GetByIdWithIncludesAsync(id, f => f.LineItems);
        if (form == null) return null;
        
        var dto = _mapper.Map<AdvanceReturnFormResponseDto>(form);
        await IncludeAttachments(dto);
        return dto;
    }

    public async Task<PagedResponseDto<AdvanceReturnFormResponseDto>> GetByAssignmentIdAsync(
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

        return new PagedResponseDto<AdvanceReturnFormResponseDto>
        {
            Items = _mapper.Map<IEnumerable<AdvanceReturnFormResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task IncludeAttachments(AdvanceReturnFormResponseDto dto)
    {
        var attachments = await _fileStorage.GetAttachmentsForEntityAsync(nameof(AdvanceReturnForm), dto.Id);
        dto.Attachments = _mapper.Map<ICollection<AttachmentDto>>(attachments).ToList();
        
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            foreach (var attachment in dto.Attachments)
            {
                attachment.FileUrl = $"{request.Scheme}://{request.Host}/api/advance-returns/attachments/{attachment.Id}";
            }
        }
    }

    public async Task<AdvanceReturnFormResponseDto> CreateAsync(CreateAdvanceReturnFormDto dto)
    {
        var form = _mapper.Map<AdvanceReturnForm>(dto);
        form.Id = Guid.NewGuid().ToString();

        // Create line items
        var lineItems = new List<AdvanceReturnLineItem>();
        foreach (var lineItemDto in dto.LineItems)
        {
            var lineItem = _mapper.Map<AdvanceReturnLineItem>(lineItemDto);
            lineItem.Id = Guid.NewGuid().ToString();
            lineItem.AdvanceReturnFormId = form.Id;
            lineItems.Add(lineItem);
        }

        form.LineItems = lineItems;
        form.TotalAmount = lineItems.Sum(li => li.AmountInKsh);

        var created = await _repository.CreateAsync(form);
        return _mapper.Map<AdvanceReturnFormResponseDto>(created);
    }

    public async Task<AdvanceReturnFormResponseDto?> UpdateAsync(string id, UpdateAdvanceReturnFormDto dto)
    {
        var existing = await _repository.GetByIdWithIncludesAsync(id, f => f.LineItems);
        if (existing == null) return null;

        if (dto.LineItems != null)
        {
            // Delete existing line items
            foreach (var lineItem in existing.LineItems.ToList())
            {
                await _lineItemRepository.DeleteAsync(lineItem.Id);
            }

            // Create new line items
            var newLineItems = new List<AdvanceReturnLineItem>();
            foreach (var lineItemDto in dto.LineItems)
            {
                var lineItem = _mapper.Map<AdvanceReturnLineItem>(lineItemDto);
                lineItem.Id = Guid.NewGuid().ToString();
                lineItem.AdvanceReturnFormId = id;
                newLineItems.Add(await _lineItemRepository.CreateAsync(lineItem));
            }

            existing.LineItems = newLineItems;
            existing.TotalAmount = newLineItems.Sum(li => li.AmountInKsh);
        }

        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<AdvanceReturnFormResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<AdvanceReturnFormResponseDto?> ApproveAsync(string id, string approvedBy, string? comments = null)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != AdvanceReturnStatus.Pending)
            return null;

        form.Status = AdvanceReturnStatus.Approved;
        form.ApprovedAt = DateTime.UtcNow;
        form.ApprovedBy = approvedBy;
        form.ApprovalComments = comments;

        var updated = await _repository.UpdateAsync(form);
        if (updated != null)
        {
            // Recalculate balance after approval
            await _balanceService.RecalculateOnFormChangeAsync(form.AssignmentId);
        }

        return updated == null ? null : _mapper.Map<AdvanceReturnFormResponseDto>(updated);
    }

    public async Task<AdvanceReturnFormResponseDto?> RejectAsync(string id, string rejectedBy, string rejectionReason)
    {
        var form = await _repository.GetByIdAsync(id);
        if (form == null) return null;

        if (form.Status != AdvanceReturnStatus.Pending)
            return null;

        form.Status = AdvanceReturnStatus.Rejected;
        form.RejectedAt = DateTime.UtcNow;
        form.RejectedBy = rejectedBy;
        form.RejectionReason = rejectionReason;

        var updated = await _repository.UpdateAsync(form);
        return updated == null ? null : _mapper.Map<AdvanceReturnFormResponseDto>(updated);
    }
}
