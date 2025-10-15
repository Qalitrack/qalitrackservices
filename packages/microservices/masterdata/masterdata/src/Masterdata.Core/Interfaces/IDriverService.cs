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
    
    /// <summary>
    /// Removes a vehicle from a driver's assigned vehicles
    /// </summary>
    /// <param name="driverId">The ID of the driver</param>
    /// <param name="vehicleId">The ID of the vehicle to remove</param>
    /// <returns>True if the operation was successful, false otherwise</returns>
    Task<bool> RemoveVehicleAsync(string driverId, string vehicleId);
}