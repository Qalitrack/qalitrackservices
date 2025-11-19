using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.Claim;

namespace TechnicianApi.Core.Interfaces;

public interface IClaimService
{
    Task<ClaimResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<ClaimResponseDto>> GetByAssignmentIdAsync(string assignmentId, int pageNumber = 1, int pageSize = 10);
    Task<ClaimResponseDto> CreateAsync(CreateClaimDto dto);
    Task<ClaimResponseDto?> UpdateAsync(string id, UpdateClaimDto dto);
    Task<bool> DeleteAsync(string id);
    Task<ClaimResponseDto?> ManagerApproveAsync(string id, ApproveClaimDto dto);
    Task<ClaimResponseDto?> ManagerRejectAsync(string id, RejectClaimDto dto);
    Task<ClaimResponseDto?> CfoApproveAsync(string id, ApproveClaimDto dto);
    Task<ClaimResponseDto?> CfoRejectAsync(string id, RejectClaimDto dto);
    Task<ClaimResponseDto?> DisburseAsync(string id, DisburseClaimDto dto);
}
