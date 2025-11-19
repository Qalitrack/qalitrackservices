using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.AdvanceReturn;

namespace TechnicianApi.Core.Interfaces;

public interface IAdvanceReturnFormService
{
    Task<AdvanceReturnFormResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<AdvanceReturnFormResponseDto>> GetByAssignmentIdAsync(string assignmentId, int pageNumber = 1, int pageSize = 10);
    Task<AdvanceReturnFormResponseDto> CreateAsync(CreateAdvanceReturnFormDto dto);
    Task<AdvanceReturnFormResponseDto?> UpdateAsync(string id, UpdateAdvanceReturnFormDto dto);
    Task<bool> DeleteAsync(string id);
    Task<AdvanceReturnFormResponseDto?> ApproveAsync(string id, string approvedBy, string? comments = null);
    Task<AdvanceReturnFormResponseDto?> RejectAsync(string id, string rejectedBy, string rejectionReason);
}
