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

            // First get the paged owners
            var pagedResult = await _ownerRepository.GetPagedAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                searchTerm: searchTerm,
                searchProperties: new[] { nameof(Owner.Name), nameof(Owner.Type) }
            );
            
            // If no owners found, return empty result
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
            
            // Get all owner IDs for the current page
            var ownerIds = pagedResult.Items.Select(o => o.Id).ToList();
            
            // Get all vehicles for these owners in a single query
            var vehicles = await _vehicleRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                searchPredicate: v => ownerIds.Contains(v.OwnerId)
            );
            
            // Group vehicles by owner ID for easy lookup
            var vehiclesByOwnerId = vehicles.Items
                .GroupBy(v => v.OwnerId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
                
            // Map owners to DTOs and attach their vehicles
            var ownerDtos = new List<OwnerDto>();
            foreach (var owner in pagedResult.Items)
            {
                var ownerDto = _mapper.Map<OwnerDto>(owner);
                if (vehiclesByOwnerId.TryGetValue(owner.Id, out var ownerVehicles))
                {
                    // If you need to set the vehicles on the DTO, you'll need to add a property for it
                    // ownerDto.Vehicles = _mapper.Map<IEnumerable<VehicleReadDto>>(ownerVehicles);
                }
                ownerDtos.Add(ownerDto);
            }

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
            _logger.LogError(ex, "Error retrieving paged owners");
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
            
            // Get vehicles for this owner
            var vehicles = await _vehicleRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                searchTerm: null,
                searchPredicate: v => v.OwnerId == id.ToString()
            );
            
            var ownerDto = _mapper.Map<OwnerDto>(owner);
            return ownerDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving owner with ID: {id}");
            throw;
        }
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto)
    {
        try
        {
            if (await IsNameAvailableAsync(dto.Name) == false)
            {
                throw new InvalidOperationException($"An owner with name '{dto.Name}' already exists.");
            }

            var owner = _mapper.Map<Owner>(dto);
            var createdOwner = await _ownerRepository.CreateAsync(owner);
            return _mapper.Map<OwnerDto>(createdOwner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating owner");
            throw;
        }
    }

    public async Task<OwnerDto?> UpdateAsync(Guid id, UpdateOwnerDto dto)
    {
        try
        {
            var owners = await _ownerRepository.GetByIdsAsync(new[] { id.ToString() });
            var existingOwner = owners.FirstOrDefault();
            if (existingOwner == null)
            {
                return null;
            }

            if (await IsNameAvailableAsync(dto.Name, id) == false)
            {
                throw new InvalidOperationException($"An owner with name '{dto.Name}' already exists.");
            }

            _mapper.Map(dto, existingOwner);
            var updatedOwner = await _ownerRepository.UpdateAsync(existingOwner);
            return updatedOwner != null ? _mapper.Map<OwnerDto>(updatedOwner) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating owner with ID: {id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // First check if owner exists
            var owners = await _ownerRepository.GetByIdsAsync(new[] { id.ToString() });
            var owner = owners.FirstOrDefault();
            if (owner == null)
            {
                return false;
            }

            // Check if owner has any vehicles
            var vehicles = await _vehicleRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,  // We only need to know if there are any vehicles
                searchPredicate: v => v.OwnerId == id.ToString()
            );

            if (vehicles.TotalItems > 0)
            {
                throw new InvalidOperationException("Cannot delete owner with associated vehicles. Please reassign or delete the vehicles first.");
            }

            return await _ownerRepository.DeleteAsync(id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting owner with ID: {id}");
            throw;
        }
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var result = await _ownerRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchTerm: name,
                searchProperties: new[] { nameof(Owner.Name) }
            );

            if (!result.Items.Any())
            {
                return true;
            }

            if (excludeId.HasValue)
            {
                return result.Items.All(o => o.Id == excludeId.Value.ToString());
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking owner name availability for: {name}");
            throw;
        }
    }

    public async Task<bool> AssignVehiclesAsync(Guid ownerId, IEnumerable<Guid> vehicleIds)
    {
        try
        {
            var vehicleIdList = vehicleIds.ToList();
            if (!vehicleIdList.Any())
            {
                return true; // No vehicles to assign, return success
            }

            // Verify owner exists
            var owners = await _ownerRepository.GetByIdsAsync(new[] { ownerId.ToString() });
            if (!owners.Any())
            {
                return false;
            }

            // Get all vehicles to update
            var vehicles = await _vehicleRepository.GetByIdsAsync(vehicleIdList.Select(id => id.ToString()).ToArray());
            if (!vehicles.Any())
            {
                return false;
            }

            // Update owner for each vehicle
            foreach (var vehicle in vehicles)
            {
                vehicle.OwnerId = ownerId.ToString();
                await _vehicleRepository.UpdateAsync(vehicle);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning vehicles to owner with ID: {ownerId}");
            throw;
        }
    }

    public async Task<bool> RemoveVehiclesAsync(Guid ownerId, IEnumerable<Guid> vehicleIds)
    {
        try
        {
            var vehicleIdList = vehicleIds.ToList();
            if (!vehicleIdList.Any())
            {
                return true; // No vehicles to remove, return success
            }

            // Get all vehicles to update
            var vehicles = await _vehicleRepository.GetByIdsAsync(vehicleIdList.Select(id => id.ToString()).ToArray());
            if (!vehicles.Any())
            {
                return false;
            }

            // Verify all vehicles belong to this owner
            if (vehicles.Any(v => v.OwnerId != ownerId.ToString()))
            {
                throw new InvalidOperationException("One or more vehicles do not belong to the specified owner.");
            }

            // Set OwnerId to empty string (you might want to handle this differently based on your requirements)
            foreach (var vehicle in vehicles)
            {
                vehicle.OwnerId = string.Empty; // Or set to a default owner if applicable
                await _vehicleRepository.UpdateAsync(vehicle);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing vehicles from owner with ID: {ownerId}");
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
                searchTerm: null,
                searchPredicate: v => v.OwnerId == ownerId.ToString()
            );

            return _mapper.Map<IEnumerable<VehicleReadDto>>(vehicles.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving vehicles for owner with ID: {ownerId}");
            throw;
        }
    }
}
