using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.PettyCash;

namespace TechnicianApi.Core.Interfaces;

public interface IPettyCashAdvanceFormService
{
    Task<PettyCashAdvanceFormResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<PettyCashAdvanceFormResponseDto>> GetByAssignmentIdAsync(string assignmentId, int pageNumber = 1, int pageSize = 10);
    Task<PettyCashAdvanceFormResponseDto> CreateAsync(CreatePettyCashAdvanceFormDto dto);
    Task<PettyCashAdvanceFormResponseDto?> UpdateAsync(string id, UpdatePettyCashAdvanceFormDto dto);
    Task<bool> DeleteAsync(string id);
    Task<PettyCashAdvanceFormResponseDto?> ApproveAsync(string id, string approvedBy, string? comments = null);
    Task<PettyCashAdvanceFormResponseDto?> RejectAsync(string id, string rejectedBy, string rejectionReason);
    Task<PettyCashAdvanceFormResponseDto?> DisburseAsync(string id, string disbursedBy, string voucherNumber, string? referenceNumber = null);
}
