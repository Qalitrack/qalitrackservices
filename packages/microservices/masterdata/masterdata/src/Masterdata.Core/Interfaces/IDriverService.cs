using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IDriverService
{
    Task<PagedResult<DriverReadDto>> GetPagedDriversAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<DriverReadDto?> GetByIdAsync(string id);
    Task<DriverReadDto> CreateAsync(CreateDriverDto dto);
    Task<DriverReadDto?> UpdateAsync(string id, UpdateDriverDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsLicenseNumberAvailableAsync(string licenseNumber);
    Task AssignVehicleAsync(string driverId, string vehicleId);
    Task<bool> RemoveVehicleAsync(string driverId, string vehicleId);
    Task<bool> AssignToSupplierAsync(string driverId, string supplierId);
    Task<bool> RemoveFromSupplierAsync(string driverId, string supplierId);
    Task<bool> AssignToTransporterAsync(string driverId, string transporterId);
    Task<bool> RemoveFromTransporterAsync(string driverId, string transporterId);
    
    // NEW: NFC Code methods
    Task<DriverReadDto?> GetByNfcCodeAsync(string nfcCode);
    Task<bool> IsNfcCodeAvailableAsync(string nfcCode, string? excludeDriverId = null);
}