using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.Refund;

namespace TechnicianApi.Core.Interfaces;

public interface IRefundService
{
    Task<RefundResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<RefundResponseDto>> GetByAssignmentIdAsync(string assignmentId, int pageNumber = 1, int pageSize = 10);
    Task<RefundResponseDto> CreateAsync(CreateRefundDto dto);
    Task<RefundResponseDto?> UpdateAsync(string id, UpdateRefundDto dto);
    Task<bool> DeleteAsync(string id);
    Task<RefundResponseDto?> ManagerApproveAsync(string id, ApproveRefundDto dto);
    Task<RefundResponseDto?> ManagerRejectAsync(string id, RejectRefundDto dto);
    Task<RefundResponseDto?> CfoConfirmReceivedAsync(string id, ConfirmRefundReceivedDto dto);
    Task<RefundResponseDto?> CfoRejectAsync(string id, RejectRefundDto dto);
}
