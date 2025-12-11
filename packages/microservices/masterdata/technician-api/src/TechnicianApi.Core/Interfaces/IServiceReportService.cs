using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.ServiceReport;

namespace TechnicianApi.Core.Interfaces;

public interface IServiceReportService
{
    Task<ServiceReportResponseDto?> GetByIdAsync(string id);
    Task<ServiceReportResponseDto?> GetByAssignmentIdAsync(string assignmentId);
    Task<PagedResponseDto<ServiceReportResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? technicianId = null, string? status = null);
    Task<ServiceReportResponseDto> CreateAsync(CreateServiceReportDto dto);
    Task<ServiceReportResponseDto?> UpdateAsync(string id, UpdateServiceReportDto dto);
    Task<bool> DeleteAsync(string id);
    Task<ServiceReportResponseDto?> SubmitReportAsync(string id);
    Task<ServiceReportResponseDto?> ApproveReportAsync(string id, string approvedBy);
    Task<ServiceReportResponseDto?> RejectReportAsync(string id, string rejectionReason);
    Task<ServiceReportResponseDto> AutoPopulateFromAssignmentAsync(string assignmentId, string? technicianId = null);
    Task CalculateFieldJobTimeAsync(string serviceReportId);
}
