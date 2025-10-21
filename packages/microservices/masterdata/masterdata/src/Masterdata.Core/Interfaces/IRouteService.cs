using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Route;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IRouteService
{
    Task<PagedResult<RouteDto>> GetPagedRoutesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<RouteDto?> GetByIdAsync(Guid id);
    Task<RouteDto> CreateAsync(CreateRouteDto dto);
    Task<RouteDto?> UpdateAsync(Guid id, UpdateRouteDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null);
}
