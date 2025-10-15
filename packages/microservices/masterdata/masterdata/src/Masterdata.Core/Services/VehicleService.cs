using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.Core.Services;

public class VehicleService : IVehicleService
{
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly IRepository<Driver> _driverRepository;
    private readonly IDriverVehicleRepository _driverVehicleRepository;
    private readonly IMapper _mapper;

    public VehicleService(
        IRepository<Vehicle> vehicleRepository,
        IRepository<Driver> driverRepository,
        IDriverVehicleRepository driverVehicleRepository,
        IMapper mapper)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _driverVehicleRepository = driverVehicleRepository ?? throw new ArgumentNullException(nameof(driverVehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<VehicleReadDto?> GetByIdAsync(string id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null || vehicle.IsDeleted) return null;

        var dto = _mapper.Map<VehicleReadDto>(vehicle);
        
        // Get assigned drivers
        dto.AssignedDriverIds = (await _driverVehicleRepository.GetVehicleDriversAsync(id)).ToList();
        
        return dto;
    }

    public async Task<PagedResult<VehicleReadDto>> GetPagedVehiclesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        var pagedResult = await _vehicleRepository.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: searchTerm,
            searchPredicate: searchTerm != null ? 
                (v => v.RegistrationNumber.Contains(searchTerm) || v.Model.Contains(searchTerm)) : 
                null,
            searchProperties: new[] { nameof(Vehicle.RegistrationNumber), nameof(Vehicle.Model) }
        );

        var vehicleDtos = _mapper.Map<IEnumerable<VehicleReadDto>>(pagedResult.Items);

        // Get assigned drivers for all vehicles
        foreach (var dto in vehicleDtos)
        {
            dto.AssignedDriverIds = (await _driverVehicleRepository.GetVehicleDriversAsync(dto.Id)).ToList();
        }

        return new PagedResult<VehicleReadDto>
        {
            Items = vehicleDtos,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems
        };
    }

    public async Task<VehicleReadDto> CreateAsync(CreateVehicleDto dto)
    {
        // Check if registration number is available
        if (!await IsRegistrationNumberAvailableAsync(dto.RegistrationNumber))
        {
            throw new InvalidOperationException($"A vehicle with registration number {dto.RegistrationNumber} already exists.");
        }

        var vehicle = _mapper.Map<Vehicle>(dto);
        var createdVehicle = await _vehicleRepository.CreateAsync(vehicle);
        
        return await GetByIdAsync(createdVehicle.Id) ?? 
            throw new InvalidOperationException("Failed to create vehicle");
    }

    public async Task<VehicleReadDto?> UpdateAsync(string id, UpdateVehicleDto dto)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return null;
        }

        // Check if registration number is available (excluding current vehicle)
        if (!await IsRegistrationNumberAvailableAsync(dto.RegistrationNumber, id))
        {
            throw new InvalidOperationException($"A different vehicle with registration number {dto.RegistrationNumber} already exists.");
        }

        _mapper.Map(dto, vehicle);
        vehicle.UpdatedAt = DateTime.UtcNow;

        var updatedVehicle = await _vehicleRepository.UpdateAsync(vehicle);
        return updatedVehicle == null ? null : await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        // Mark as deleted
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;

        // Clear all driver assignments
        await _driverVehicleRepository.RemoveAllDriverAssignmentsAsync(id);
        
        // Clear primary driver reference
        if (vehicle.DriverId != null)
        {
            vehicle.DriverId = null;
        }

        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> IsRegistrationNumberAvailableAsync(string registrationNumber, string? excludeVehicleId = null)
    {
        var result = await _vehicleRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            searchPredicate: v => 
                v.RegistrationNumber == registrationNumber && 
                !v.IsDeleted &&
                (excludeVehicleId == null || v.Id != excludeVehicleId),
            searchProperties: new[] { nameof(Vehicle.RegistrationNumber) }
        );
        
        return !result.Items.Any();
    }

    public async Task<bool> AssignDriverAsync(string vehicleId, string? driverId)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        // If driverId is null, just clear the primary driver
        if (string.IsNullOrEmpty(driverId))
        {
            vehicle.DriverId = null;
            vehicle.UpdatedAt = DateTime.UtcNow;
            
            var result = await _vehicleRepository.UpdateAsync(vehicle);
            return result != null;
        }

        // Verify the driver exists and is not deleted
        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Check if the driver is already assigned to the vehicle
        var isAssigned = await _driverVehicleRepository.IsVehicleAssignedToDriverAsync(driverId, vehicleId);
        if (!isAssigned)
        {
            // If not assigned, assign the vehicle to the driver
            await _driverVehicleRepository.AssignVehicleToDriverAsync(driverId, vehicleId);
        }

        // Update the primary driver reference
        vehicle.DriverId = driverId;
        vehicle.UpdatedAt = DateTime.UtcNow;
        
        var updateResult = await _vehicleRepository.UpdateAsync(vehicle);
        return updateResult != null;
    }

    public async Task<bool> RemoveDriverAsync(string vehicleId, string driverId)
    {
        if (string.IsNullOrEmpty(vehicleId) || string.IsNullOrEmpty(driverId))
        {
            return false;
        }

        // Check if the vehicle exists and is not deleted
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        // Check if the driver is actually assigned to the vehicle
        var isAssigned = await _driverVehicleRepository.IsVehicleAssignedToDriverAsync(driverId, vehicleId);
        if (!isAssigned)
        {
            // If not assigned, consider it a success (idempotent operation)
            return true;
        }

        // Remove the driver assignment
        var success = await _driverVehicleRepository.RemoveVehicleFromDriverAsync(driverId, vehicleId);
        
        // Clear primary driver reference if this driver was set as primary
        if (success && vehicle.DriverId == driverId)
        {
            vehicle.DriverId = null;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        return success;
    }

    public async Task<IEnumerable<string>> GetAssignedDriversAsync(string vehicleId)
    {
        if (string.IsNullOrEmpty(vehicleId))
        {
            return Enumerable.Empty<string>();
        }

        // Check if the vehicle exists and is not deleted
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return Enumerable.Empty<string>();
        }

        // Get all driver IDs assigned to this vehicle
        return await _driverVehicleRepository.GetVehicleDriversAsync(vehicleId);
    }

    public async Task<bool> UpdateStatusAsync(string vehicleId, string status)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.Status = status;
        vehicle.UpdatedAt = DateTime.UtcNow;
        
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> AssignToSupplierAsync(string vehicleId, string? supplierId)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.SupplierId = supplierId;
        vehicle.UpdatedAt = DateTime.UtcNow;
        
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> AssignToTransporterAsync(string vehicleId, string? transporterId)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.TransporterId = transporterId;
        vehicle.UpdatedAt = DateTime.UtcNow;
        
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> AssignToOwnerAsync(string vehicleId, string? ownerId)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.OwnerId = ownerId;
        vehicle.UpdatedAt = DateTime.UtcNow;
        
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }
}