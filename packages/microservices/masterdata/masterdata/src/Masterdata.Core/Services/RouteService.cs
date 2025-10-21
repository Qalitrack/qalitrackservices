using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Route;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.Core.Services;

public class RouteService : IRouteService
{
    private readonly IRepository<Route> _routeRepository;
    private readonly IMapper _mapper;

    public RouteService(
        IRepository<Route> routeRepository,
        IMapper mapper)
    {
        _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<RouteDto>> GetPagedRoutesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        // Ensure page number and size are valid
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Get paginated routes with search
        var pagedResult = await _routeRepository.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: searchTerm,
            searchProperties: new[] { nameof(Route.Name), nameof(Route.StartPoint), nameof(Route.EndPoint) }
        );

        if (!pagedResult.Items.Any())
        {
            return new PagedResult<RouteDto>
            {
                Items = Enumerable.Empty<RouteDto>(),
                TotalItems = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        var items = pagedResult.Items.Select(r => _mapper.Map<RouteDto>(r)).ToList();

        return new PagedResult<RouteDto>
        {
            Items = items,
            TotalItems = pagedResult.TotalItems,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<RouteDto?> GetByIdAsync(Guid id)
    {
        var routes = await _routeRepository.GetByIdsAsync(new[] { id.ToString() });
        var route = routes.FirstOrDefault();
        return route != null ? _mapper.Map<RouteDto>(route) : null;
    }

    public async Task<RouteDto> CreateAsync(CreateRouteDto dto)
    {
        if (await IsNameAvailableAsync(dto.Name) == false)
        {
            throw new InvalidOperationException($"A route with name '{dto.Name}' already exists.");
        }

        var route = _mapper.Map<Route>(dto);
        var createdRoute = await _routeRepository.CreateAsync(route);
        return _mapper.Map<RouteDto>(createdRoute);
    }

    public async Task<RouteDto?> UpdateAsync(Guid id, UpdateRouteDto dto)
    {
        var routes = await _routeRepository.GetByIdsAsync(new[] { id.ToString() });
        var existingRoute = routes.FirstOrDefault();
        if (existingRoute == null)
        {
            return null;
        }

        if (await IsNameAvailableAsync(dto.Name, id) == false)
        {
            throw new InvalidOperationException($"A route with name '{dto.Name}' already exists.");
        }

        _mapper.Map(dto, existingRoute);
        var updatedRoute = await _routeRepository.UpdateAsync(existingRoute);
        return updatedRoute != null ? _mapper.Map<RouteDto>(updatedRoute) : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _routeRepository.DeleteAsync(id.ToString());
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        // Get all routes with the same name (case-insensitive)
        var result = await _routeRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            searchTerm: name,
            searchProperties: new[] { nameof(Route.Name) }
        );

        // If no route found with this name, it's available
        if (!result.Items.Any())
        {
            return true;
        }

        // If checking for a specific route (update case), exclude it from the check
        if (excludeId.HasValue)
        {
            return result.Items.All(r => r.Id == excludeId.Value.ToString());
        }

        return false;
    }
}
