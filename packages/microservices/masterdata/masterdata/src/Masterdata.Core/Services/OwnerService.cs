using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Owner;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class OwnerService : IOwnerService
{
    private readonly IRepository<Owner> _ownerRepository;
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(
        IRepository<Owner> ownerRepository,
        IRepository<Vehicle> vehicleRepository,
        IMapper mapper,
        ILogger<OwnerService> logger)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<OwnerDto>> GetPagedOwnersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var pagedResult = await _ownerRepository.GetPagedAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                searchTerm: searchTerm,
                searchProperties: new[] { nameof(Owner.Name), nameof(Owner.Type) }
            );

            if (!pagedResult.Items.Any())
            {
                return new PagedResult<OwnerDto>
                {
                    Items = Enumerable.Empty<OwnerDto>(),
                    TotalItems = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var items = pagedResult.Items.Select(o => _mapper.Map<OwnerDto>(o)).ToList();

            return new PagedResult<OwnerDto>
            {
                Items = items,
                TotalItems = pagedResult.TotalItems,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged owners (page {PageNumber}, size {PageSize}, search: {SearchTerm})", 
                pageNumber, pageSize, searchTerm);
            throw;
        }
    }

    public async Task<OwnerDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var owners = await _ownerRepository.GetByIdsAsync(new[] { id.ToString() });
            var owner = owners.FirstOrDefault();
            if (owner == null) return null;

            return _mapper.Map<OwnerDto>(owner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving owner with ID: {OwnerId}", id);
            throw;
        }
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto)
    {
        try
        {
            if (!await IsNameAvailableAsync(dto.Name))
            {
                throw new InvalidOperationException($"An owner with name '{dto.Name}' already exists.");
            }

            var owner = _mapper.Map<Owner>(dto);
            var created = await _ownerRepository.CreateAsync(owner);

            return _mapper.Map<OwnerDto>(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating owner with name: {Name}", dto.Name);
            throw;
        }
    }

    public async Task<OwnerDto?> UpdateAsync(Guid id, UpdateOwnerDto dto)
    {
        try
        {
            var owners = await _ownerRepository.GetByIdsAsync(new[] { id.ToString() });
            var existing = owners.FirstOrDefault();
            if (existing == null) return null;

            if (!await IsNameAvailableAsync(dto.Name, id))
            {
                throw new InvalidOperationException($"An owner with name '{dto.Name}' already exists.");
            }

            _mapper.Map(dto, existing);
            var updated = await _ownerRepository.UpdateAsync(existing);

            return updated != null ? _mapper.Map<OwnerDto>(updated) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating owner with ID: {OwnerId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var owners = await _ownerRepository.GetByIdsAsync(new[] { id.ToString() });
            if (!owners.Any()) return false;

            // Check for associated vehicles
            var vehiclesCheck = await _vehicleRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchPredicate: v => v.OwnerId == id.ToString()
            );

            if (vehiclesCheck.TotalItems > 0)
            {
                throw new InvalidOperationException("Cannot delete owner with associated vehicles. Reassign vehicles first.");
            }

            return await _ownerRepository.DeleteAsync(id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting owner with ID: {OwnerId}", id);
            throw;
        }
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        try
        {
            var result = await _ownerRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchTerm: name,
                searchProperties: new[] { nameof(Owner.Name) }
            );

            if (!result.Items.Any()) return true;

            return excludeId.HasValue && result.Items.All(o => o.Id == excludeId.Value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking name availability for: '{Name}'", name);
            throw;
        }
    }

    public async Task<bool> AssignVehiclesAsync(Guid ownerId, IEnumerable<Guid> vehicleIds)
    {
        var vehicleIdList = vehicleIds?.ToList() ?? new List<Guid>();
        if (!vehicleIdList.Any()) return true;

        try
        {
            // Check owner exists
            if (!await _ownerRepository.ExistsAsync(ownerId.ToString()))
            {
                return false;
            }

            // Fetch vehicles
            var vehicles = await _vehicleRepository.GetByIdsAsync(
                vehicleIdList.Select(id => id.ToString()).ToArray());

            if (!vehicles.Any()) return false;

            // Prevent reassigning vehicles that belong to someone else
            var alreadyOwned = vehicles
                .Where(v => !string.IsNullOrEmpty(v.OwnerId) && v.OwnerId != ownerId.ToString())
                .Select(v => v.RegistrationNumber ?? v.Id)
                .ToList();

            if (alreadyOwned.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot assign vehicles already owned by another owner: {string.Join(", ", alreadyOwned)}");
            }

            // Bulk update
            foreach (var vehicle in vehicles)
            {
                vehicle.OwnerId = ownerId.ToString();
            }

            // Assuming your repository supports bulk update
            // If not, use transaction + SaveChanges in one go
            await _vehicleRepository.UpdateRangeAsync(vehicles);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning vehicles to owner {OwnerId}", ownerId);
            throw;
        }
    }

    public async Task<bool> RemoveVehiclesAsync(Guid ownerId, IEnumerable<Guid> vehicleIds)
    {
        var vehicleIdList = vehicleIds?.ToList() ?? new List<Guid>();
        if (!vehicleIdList.Any()) return true;

        try
        {
            var vehicles = await _vehicleRepository.GetByIdsAsync(
                vehicleIdList.Select(id => id.ToString()).ToArray());

            if (!vehicles.Any()) return false;

            // Security check: only remove vehicles actually belonging to this owner
            if (vehicles.Any(v => v.OwnerId != ownerId.ToString()))
            {
                throw new InvalidOperationException("One or more vehicles do not belong to the specified owner.");
            }

            foreach (var vehicle in vehicles)
            {
                vehicle.OwnerId = null;  // ← null instead of empty string
            }

            await _vehicleRepository.UpdateRangeAsync(vehicles);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing vehicles from owner {OwnerId}", ownerId);
            throw;
        }
    }

    public async Task<IEnumerable<VehicleReadDto>> GetOwnerVehiclesAsync(Guid ownerId)
    {
        try
        {
            var vehicles = await _vehicleRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                searchPredicate: v => v.OwnerId == ownerId.ToString()
            );

            return _mapper.Map<IEnumerable<VehicleReadDto>>(vehicles.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles for owner {OwnerId}", ownerId);
            throw;
        }
    }
}