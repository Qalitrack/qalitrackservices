using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IVehicleService
{
    // Basic CRUD operations
    Task<VehicleReadDto?> GetByIdAsync(string id);
    Task<PagedResult<VehicleReadDto>> GetPagedVehiclesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<VehicleReadDto> CreateAsync(CreateVehicleDto dto);
    Task<VehicleReadDto?> UpdateAsync(string id, UpdateVehicleDto dto);
    Task<bool> DeleteAsync(string id);
    
    // Vehicle-specific operations
    Task<bool> IsRegistrationNumberAvailableAsync(string registrationNumber, string? excludeVehicleId = null);
    Task<bool> AssignDriverAsync(string vehicleId, string? driverId);
    Task<bool> RemoveDriverAsync(string vehicleId, string driverId);
    Task<IEnumerable<string>> GetAssignedDriversAsync(string vehicleId);
    
    // Status management
    Task<bool> UpdateStatusAsync(string vehicleId, string status);
    
    // Assignment management
    Task<bool> AssignToSupplierAsync(string vehicleId, string? supplierId);
    Task<bool> AssignToTransporterAsync(string vehicleId, string? transporterId);
    Task<bool> AssignToOwnerAsync(string vehicleId, string? ownerId);
}