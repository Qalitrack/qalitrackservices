using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.Requisition;

namespace TechnicianApi.Core.Interfaces;

public interface IRequisitionService
{
    Task<RequisitionResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<RequisitionResponseDto>> GetByAssignmentIdAsync(string assignmentId);
    Task<PagedResponseDto<RequisitionResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? technicianId = null, string? status = null);
    Task<RequisitionResponseDto> CreateAsync(CreateRequisitionDto dto);
    Task<RequisitionResponseDto?> UpdateAsync(string id, UpdateRequisitionDto dto);
    Task<bool> DeleteAsync(string id);
    Task<RequisitionResponseDto?> TmApproveAsync(string id, string tmId, string? comments);
    Task<RequisitionResponseDto?> TmRejectAsync(string id, string tmId, string rejectionReason);
    Task<RequisitionResponseDto?> CfoApproveAsync(string id, string cfoId, string? comments);
    Task<RequisitionResponseDto?> CfoRejectAsync(string id, string cfoId, string rejectionReason);
    Task<RequisitionResponseDto?> MarkAsPaidAsync(string id, string voucherNumber, string referenceNumber);
}
