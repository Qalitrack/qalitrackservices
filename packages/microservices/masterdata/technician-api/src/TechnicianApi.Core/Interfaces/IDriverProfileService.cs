using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IDriverProfileService
{
    Task<DriverProfileResponseDto?> GetByIdAsync(string id);
    Task<DriverProfileResponseDto?> GetCurrentProfileByDriverIdAsync(string driverId);
    Task<PagedResponseDto<DriverProfileResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? driverId = null, string? status = null);
    Task<DriverProfileResponseDto> CreateAsync(CreateDriverProfileDto dto);
    Task<DriverProfileResponseDto?> UpdateAsync(string id, UpdateDriverProfileDto dto);
    Task<bool> DeleteAsync(string id);
    Task<DriverProfileResponseDto?> SubmitForApprovalAsync(string id);
    Task<DriverProfileResponseDto?> ApproveProfileAsync(string id, ApproveDriverProfileDto dto, string reviewedBy);
    Task<IEnumerable<DriverProfileResponseDto>> GetProfileHistoryAsync(string driverId);
    Task<IEnumerable<DriverProfileResponseDto>> GetExpiringLicensesAsync(int daysThreshold);
    Task<DriverProfileResponseDto?> UpdateProfilePhotoAsync(string id, string photoUrl);
    Task<DriverProfileResponseDto?> UpdateLicenseFrontAsync(string id, string imageUrl);
    Task<DriverProfileResponseDto?> UpdateLicenseBackAsync(string id, string imageUrl);
    Task<DriverProfileResponseDto?> UpdateIdFrontAsync(string id, string imageUrl);
    Task<DriverProfileResponseDto?> UpdateIdBackAsync(string id, string imageUrl);
}
