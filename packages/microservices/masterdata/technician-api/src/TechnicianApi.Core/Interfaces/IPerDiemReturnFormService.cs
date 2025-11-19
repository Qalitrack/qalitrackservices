using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.PerDiemReturn;

namespace TechnicianApi.Core.Interfaces;

public interface IPerDiemReturnFormService
{
    Task<PerDiemReturnFormResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<PerDiemReturnFormResponseDto>> GetByAssignmentIdAsync(string assignmentId, int pageNumber = 1, int pageSize = 10);
    Task<PerDiemReturnFormResponseDto> CreateAsync(CreatePerDiemReturnFormDto dto);
    Task<PerDiemReturnFormResponseDto?> UpdateAsync(string id, UpdatePerDiemReturnFormDto dto);
    Task<bool> DeleteAsync(string id);
    Task<PerDiemReturnFormResponseDto?> ApproveAsync(string id, string approvedBy, string? comments = null);
    Task<PerDiemReturnFormResponseDto?> RejectAsync(string id, string rejectedBy, string rejectionReason);
}
