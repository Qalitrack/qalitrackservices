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
    Task<IEnumerable<string>> GetAssignedDriversAsync(string vehicleId);
    
    // Status management
    Task<bool> UpdateStatusAsync(string vehicleId, string status);
    
    // Assignment management
    Task<bool> AssignToSupplierAsync(string vehicleId, string? supplierId);
    Task<bool> AssignToTransporterAsync(string vehicleId, string? transporterId);
    Task<bool> AssignToOwnerAsync(string vehicleId, string? ownerId);
    Task<IEnumerable<VehicleReadDto>> GetVehiclesBySupplierIdAsync(string supplierId);

    // NEW: RFID Code methods
    /// <summary>
    /// Get a vehicle by its RFID code
    /// </summary>
    /// <param name="rfidCode">The RFID code to search for</param>
    /// <returns>The vehicle with the specified RFID code, or null if not found</returns>
    Task<VehicleReadDto?> GetByRfidCodeAsync(string rfidCode);
    
    /// <summary>
    /// Check if an RFID code is already assigned to a vehicle
    /// </summary>
    /// <param name="rfidCode">The RFID code to check</param>
    /// <param name="excludeVehicleId">Optional vehicle ID to exclude from the check (for updates)</param>
    /// <returns>True if the RFID code is available, false if already in use</returns>
    Task<bool> IsRfidCodeAvailableAsync(string rfidCode, string? excludeVehicleId = null);
}