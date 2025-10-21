using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Owner;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IOwnerService
{
    Task<PagedResult<OwnerDto>> GetPagedOwnersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<OwnerDto?> GetByIdAsync(Guid id);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerDto?> UpdateAsync(Guid id, UpdateOwnerDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null);
    Task<bool> AssignVehiclesAsync(Guid ownerId, IEnumerable<Guid> vehicleIds);
    Task<bool> RemoveVehiclesAsync(Guid ownerId, IEnumerable<Guid> vehicleIds);
    Task<IEnumerable<VehicleReadDto>> GetOwnerVehiclesAsync(Guid ownerId);
}
