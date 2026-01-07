using AutoMapper;
using Microsoft.AspNetCore.Http;
using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.DTOs.Attachment;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssignmentService(
        IAssignmentRepository assignmentRepository,
        IFileStorageService fileStorage,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _assignmentRepository = assignmentRepository;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AssignmentResponseDto?> GetByIdAsync(string id)
    {
        var assignment = await _assignmentRepository.GetByIdWithTechnicianIdsAsync(id);
        if (assignment == null)
            return null;
            
        var dto = _mapper.Map<AssignmentResponseDto>(assignment);
        await IncludeAttachments(dto);
        return dto;
    }

    public async Task<PagedResponseDto<AssignmentResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null)
    {
        var (items, totalCount) = await _assignmentRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            technicianId,
            status);

        return new PagedResponseDto<AssignmentResponseDto>
        {
            Items = _mapper.Map<IEnumerable<AssignmentResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task IncludeAttachments(AssignmentResponseDto dto)
    {
        var attachments = await _fileStorage.GetAttachmentsForEntityAsync(nameof(Assignment), dto.Id);
        dto.Attachments = _mapper.Map<ICollection<AttachmentDto>>(attachments).ToList();
        
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            foreach (var attachment in dto.Attachments)
            {
                attachment.FileUrl = $"{request.Scheme}://{request.Host}/api/assignments/attachments/{attachment.Id}";
            }
        }
    }

    public async Task<AssignmentResponseDto> CreateAsync(CreateAssignmentDto dto)
    {
        var assignment = _mapper.Map<Assignment>(dto);
        var created = await _assignmentRepository.CreateWithTechnicianIdsAsync(assignment);
        var createdDto = _mapper.Map<AssignmentResponseDto>(created);
        await IncludeAttachments(createdDto);
        return createdDto;
    }

    public async Task<AssignmentResponseDto?> UpdateAsync(string id, UpdateAssignmentDto dto)
    {
        var existing = await _assignmentRepository.GetByIdWithTechnicianIdsAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);
        var updated = await _assignmentRepository.UpdateWithTechnicianIdsAsync(existing);

        return updated == null ? null : _mapper.Map<AssignmentResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _assignmentRepository.DeleteAsync(id);
    }

    public async Task<AssignmentResponseDto?> AcceptAssignmentAsync(string id, string technicianId)
    {
        var assignment = await _assignmentRepository.GetByIdWithTechnicianIdsAsync(id);

        if (assignment == null || !assignment.TechnicianIds.Contains(technicianId))
            return null;

        if (assignment.Status != AssignmentStatus.Pending)
            return null;

        assignment.Status = AssignmentStatus.Accepted;
        assignment.AcceptedAt = DateTime.UtcNow;

        var updated = await _assignmentRepository.UpdateWithTechnicianIdsAsync(assignment);
        return updated == null ? null : _mapper.Map<AssignmentResponseDto>(updated);
    }

    public async Task<AssignmentResponseDto?> DeclineAssignmentAsync(string id, string technicianId)
    {
        var assignment = await _assignmentRepository.GetByIdWithTechnicianIdsAsync(id);

        if (assignment == null || !assignment.TechnicianIds.Contains(technicianId))
            return null;

        if (assignment.Status != AssignmentStatus.Pending)
            return null;

        assignment.Status = AssignmentStatus.Declined;

        var updated = await _assignmentRepository.UpdateWithTechnicianIdsAsync(assignment);
        return updated == null ? null : _mapper.Map<AssignmentResponseDto>(updated);
    }

    public async Task<AssignmentResponseDto?> StartAssignmentAsync(string id, string technicianId)
    {
        var assignment = await _assignmentRepository.GetByIdWithTechnicianIdsAsync(id);

        if (assignment == null || !assignment.TechnicianIds.Contains(technicianId))
            return null;

        if (assignment.Status != AssignmentStatus.Accepted)
            return null;

        assignment.Status = AssignmentStatus.InProgress;
        assignment.StartedAt = DateTime.UtcNow;

        var updated = await _assignmentRepository.UpdateWithTechnicianIdsAsync(assignment);
        return updated == null ? null : _mapper.Map<AssignmentResponseDto>(updated);
    }

    public async Task<AssignmentResponseDto?> CompleteAssignmentAsync(string id, string technicianId)
    {
        var assignment = await _assignmentRepository.GetByIdWithTechnicianIdsAsync(id);

        if (assignment == null || !assignment.TechnicianIds.Contains(technicianId))
            return null;

        if (assignment.Status != AssignmentStatus.InProgress)
            return null;

        assignment.Status = AssignmentStatus.Completed;
        assignment.CompletedAt = DateTime.UtcNow;

        var updated = await _assignmentRepository.UpdateWithTechnicianIdsAsync(assignment);
        return updated == null ? null : _mapper.Map<AssignmentResponseDto>(updated);
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetByTechnicianIdAsync(string technicianId)
    {
        var assignments = await _assignmentRepository.GetByTechnicianIdAsync(technicianId);
        return _mapper.Map<IEnumerable<AssignmentResponseDto>>(assignments);
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetByManagerIdAsync(string managerId)
    {
        var assignments = await _assignmentRepository.GetByManagerIdAsync(managerId);
        return _mapper.Map<IEnumerable<AssignmentResponseDto>>(assignments);
    }

    public async Task<AssignmentResponseDto?> AssignTechnicianAsync(string assignmentId, string technicianId)
    {
        var assignment = await _assignmentRepository.AssignTechnicianAsync(assignmentId, technicianId);
        return assignment == null ? null : _mapper.Map<AssignmentResponseDto>(assignment);
    }

    public async Task<AssignmentResponseDto?> UnassignTechnicianAsync(string assignmentId, string technicianId)
    {
        var assignment = await _assignmentRepository.UnassignTechnicianAsync(assignmentId, technicianId);
        return assignment == null ? null : _mapper.Map<AssignmentResponseDto>(assignment);
    }
}
