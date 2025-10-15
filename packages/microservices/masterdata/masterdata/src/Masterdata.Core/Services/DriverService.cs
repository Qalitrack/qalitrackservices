using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.Core.Services;

public class DriverService : IDriverService
{
    private readonly IRepository<Driver> _driverRepository;
    private readonly IDriverVehicleRepository _driverVehicleRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IMapper _mapper;

    public DriverService(
        IRepository<Driver> driverRepository,
        IDriverVehicleRepository driverVehicleRepository,
        IRepository<Supplier> supplierRepository,
        IMapper mapper)
    {
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _driverVehicleRepository = 
            driverVehicleRepository ?? throw new ArgumentNullException(nameof(driverVehicleRepository));
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<DriverReadDto>> GetPagedDriversAsync(int pageNumber = 1, int pageSize = 10,
        string? searchTerm = null)
    {
        var pagedResult = await _driverRepository.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: searchTerm,
            searchPredicate: searchTerm != null
                ? (d => d.FullName.Contains(searchTerm) || d.LicenseNumber.Contains(searchTerm))
                : null,
            searchProperties: new[] { nameof(Driver.FullName), nameof(Driver.LicenseNumber) }
        );

        var driverDtos = _mapper.Map<IEnumerable<DriverReadDto>>(pagedResult.Items);

        // Get assigned vehicles for all drivers in one go if possible, or per driver
        foreach (var driverDto in driverDtos)
        {
            var vehicles = await _driverVehicleRepository.GetDriverVehiclesAsync(driverDto.Id);
            driverDto.AssignedVehicleIds = vehicles.ToList();
        }

        return new PagedResult<DriverReadDto>
        {
            Items = driverDtos,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems
        };
    }

    [Obsolete("Use GetPagedDriversAsync for better performance with large datasets")]
    public async Task<IEnumerable<DriverReadDto>> GetAllAsync()
    {
        var pagedResult = await _driverRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1000
        );

        var drivers = pagedResult.Items.ToList();
        var driverDtos = _mapper.Map<IEnumerable<DriverReadDto>>(drivers).ToList();

        // Get assigned vehicles for all drivers
        foreach (var driverDto in driverDtos)
        {
            var vehicles = await _driverVehicleRepository.GetDriverVehiclesAsync(driverDto.Id);
            driverDto.AssignedVehicleIds = vehicles.ToList();
        }

        return driverDtos;
    }

    public async Task<DriverReadDto?> GetByIdAsync(string id)
    {
        // First get the driver with basic info
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null || driver.IsDeleted)
            return null;

        // Get the assigned vehicles separately
        var vehicleIds = await _driverVehicleRepository.GetDriverVehiclesAsync(id);

        var dto = _mapper.Map<DriverReadDto>(driver);
        dto.AssignedVehicleIds = vehicleIds.ToList();

        return dto;
    }

    public async Task<DriverReadDto> CreateAsync(CreateDriverDto dto)
    {
        // Validate supplier exists if provided
        if (!string.IsNullOrEmpty(dto.SupplierId))
        {
            var supplier = await _supplierRepository.GetByIdAsync(dto.SupplierId);
            if (supplier == null)
            {
                throw new KeyNotFoundException($"Supplier with ID {dto.SupplierId} not found.");
            }
        }

        var driver = _mapper.Map<Driver>(dto);
        var createdDriver = await _driverRepository.CreateAsync(driver);
        
        var result = _mapper.Map<DriverReadDto>(createdDriver);
        result.AssignedVehicleIds = new List<string>();
        
        return result;
    }

    public async Task<DriverReadDto?> UpdateAsync(string id, UpdateDriverDto dto)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null || driver.IsDeleted)
        {
            return null;
        }

        // Update driver properties
        _mapper.Map(dto, driver);
        driver.UpdatedAt = DateTime.UtcNow;

        // Update the driver
        var updatedDriver = await _driverRepository.UpdateAsync(driver);
        if (updatedDriver == null)
        {
            return null;
        }

        // Update vehicle assignments if provided
        if (dto.AssignedVehicleIds != null)
        {
            // Get current assignments
            var currentVehicleIds = (await _driverVehicleRepository.GetDriverVehiclesAsync(id)).ToHashSet();
            var newVehicleIds = dto.AssignedVehicleIds.ToHashSet();

            // Remove unassigned vehicles
            foreach (var vehicleId in currentVehicleIds.Except(newVehicleIds))
            {
                await _driverVehicleRepository.RemoveVehicleFromDriverAsync(id, vehicleId);
            }

            // Add new assignments
            foreach (var vehicleId in newVehicleIds.Except(currentVehicleIds))
            {
                await _driverVehicleRepository.AssignVehicleToDriverAsync(id, vehicleId);
            }
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // Get the driver with their vehicle assignments
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Remove all vehicle assignments for this driver
        await _driverVehicleRepository.RemoveAllVehicleAssignmentsAsync(id);

        // Clear any primary driver references
        await _driverVehicleRepository.ClearPrimaryDriverReferencesAsync(id);

        // Mark the driver as deleted
        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;

        // Update the driver to mark as deleted
        var result = await _driverRepository.UpdateAsync(driver);
        return result != null;
    }

    public async Task<bool> IsLicenseNumberAvailableAsync(string licenseNumber)
    {
        var result = await _driverRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            searchPredicate: d => d.LicenseNumber == licenseNumber,
            searchProperties: new[] { nameof(Driver.LicenseNumber) });
        return !result.Items.Any();
    }

    public async Task AssignVehicleAsync(string driverId, string vehicleId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(vehicleId))
        {
            throw new ArgumentException("Driver ID and Vehicle ID must be provided");
        }

        var success = await _driverVehicleRepository.AssignVehicleToDriverAsync(driverId, vehicleId);
        if (!success)
        {
            throw new InvalidOperationException("Failed to assign vehicle to driver");
        }
    }

    public async Task<bool> RemoveVehicleAsync(string driverId, string vehicleId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(vehicleId))
        {
            return false;
        }

        // Check if the driver exists and is not deleted
        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Check if the vehicle is actually assigned to the driver
        var isAssigned = await _driverVehicleRepository.IsVehicleAssignedToDriverAsync(driverId, vehicleId);
        if (!isAssigned)
        {
            // If not assigned, consider it a success (idempotent operation)
            return true;
        }

        // Remove the vehicle assignment
        var success = await _driverVehicleRepository.RemoveVehicleFromDriverAsync(driverId, vehicleId);

        // Clear primary driver reference if this driver was set as primary for the vehicle
        if (success)
        {
            await _driverVehicleRepository.SetPrimaryDriverAsync(vehicleId, null);
        }

        return success;
    }
}