using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Supplier;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface ISupplierService
{
    Task<PagedResult<SupplierReadDto>> GetPagedSuppliersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null);

    Task<SupplierReadDto?> GetSupplierByIdAsync(string id);
    
    Task<SupplierReadDto> CreateSupplierAsync(CreateSupplierDto dto);
    
    Task<bool> UpdateSupplierAsync(string id, UpdateSupplierDto dto);
    
    Task<bool> DeleteSupplierAsync(string id);
    
    Task<bool> ExistsAsync(string id);
    
    /// <summary>
    /// Gets all drivers associated with a supplier
    /// </summary>
    /// <param name="supplierId">The supplier ID</param>
    /// <returns>Collection of DriverReadDto</returns>
    Task<IEnumerable<DriverReadDto>> GetDriversBySupplierIdAsync(string supplierId);
    
    /// <summary>
    /// Gets all vehicles associated with a supplier
    /// </summary>
    /// <param name="supplierId">The supplier ID</param>
    /// <returns>Collection of VehicleReadDto</returns>
    Task<IEnumerable<VehicleReadDto>> GetVehiclesBySupplierIdAsync(string supplierId);
    
    Task<IEnumerable<SupplierReadDto>> GetSuppliersByIdsAsync(IEnumerable<string> ids);
}
