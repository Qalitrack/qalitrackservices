using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IVehicleMileageService
{
    Task<VehicleMileageResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<VehicleMileageResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? truckId = null, string? driverId = null);
    Task<VehicleMileageResponseDto> CreateAsync(CreateVehicleMileageDto dto);
    Task<VehicleMileageResponseDto?> UpdateAsync(string id, CreateVehicleMileageDto dto);
    Task<bool> DeleteAsync(string id);
    Task<VehicleMileageResponseDto?> UpdateProofImageAsync(string id, string imageUrl);
    Task<VehicleMileageResponseDto?> UpdateProofEndImageAsync(string id, string imageUrl);
}
