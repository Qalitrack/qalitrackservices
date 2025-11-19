using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<AssignmentResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? technicianId = null, string? status = null);
    Task<AssignmentResponseDto> CreateAsync(CreateAssignmentDto dto);
    Task<AssignmentResponseDto?> UpdateAsync(string id, UpdateAssignmentDto dto);
    Task<bool> DeleteAsync(string id);
    Task<AssignmentResponseDto?> AcceptAssignmentAsync(string id, string technicianId);
    Task<AssignmentResponseDto?> DeclineAssignmentAsync(string id, string technicianId);
    Task<AssignmentResponseDto?> StartAssignmentAsync(string id, string technicianId);
    Task<AssignmentResponseDto?> CompleteAssignmentAsync(string id, string technicianId);
    Task<IEnumerable<AssignmentResponseDto>> GetByTechnicianIdAsync(string technicianId);
    Task<IEnumerable<AssignmentResponseDto>> GetByManagerIdAsync(string managerId);
    Task<AssignmentResponseDto?> AssignTechnicianAsync(string assignmentId, string technicianId);
    Task<AssignmentResponseDto?> UnassignTechnicianAsync(string assignmentId, string technicianId);
}
