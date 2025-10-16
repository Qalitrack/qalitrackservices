using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Transporters;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface ITransporterService
{
    Task<PagedResult<TransporterReadDto>> GetPagedTransportersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null);

    Task<TransporterReadDto?> GetTransporterByIdAsync(string id);
    
    Task<TransporterReadDto> CreateTransporterAsync(CreateTransporterDto dto);
    
    Task<bool> UpdateTransporterAsync(string id, UpdateTransporterDto dto);
    
    Task<bool> DeleteTransporterAsync(string id);
    
    Task<bool> ExistsAsync(string id);
    
    Task<IEnumerable<TransporterReadDto>> GetTransportersByIdsAsync(IEnumerable<string> ids);
    // Add these methods to ITransporterService

    /// <summary>
    /// Get all vehicles associated with a transporter
    /// </summary>
    Task<IEnumerable<VehicleReadDto>> GetTransporterVehiclesAsync(string transporterId);

    /// <summary>
    /// Get all drivers associated with a transporter
    /// </summary>
    Task<IEnumerable<DriverReadDto>> GetTransporterDriversAsync(string transporterId);
}
