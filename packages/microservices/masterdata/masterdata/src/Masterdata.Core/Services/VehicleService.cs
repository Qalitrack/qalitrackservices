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
    private readonly IRepository<DriverVehicle> _driverVehicleRepository;
    private readonly IMapper _mapper;

    public VehicleService(
        IRepository<Vehicle> vehicleRepository,
        IRepository<Driver> driverRepository,
        IRepository<DriverVehicle> driverVehicleRepository,
        IMapper mapper)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _driverVehicleRepository = driverVehicleRepository ?? throw new ArgumentNullException(nameof(driverVehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<VehicleReadDto?> GetByIdAsync(string id)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { id });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted) return null;

        var dto = _mapper.Map<VehicleReadDto>(vehicle);

        // Get assigned drivers
        var driverVehicles = await _driverVehicleRepository.GetAllByPredicateAsync(dv => dv.VehicleId == id);
        dto.AssignedDriverIds = driverVehicles
            .Select(dv => dv.DriverId)
            .ToList();

        return dto;
    }

    // NEW: Get vehicle by RFID code
    public async Task<VehicleReadDto?> GetByRfidCodeAsync(string rfidCode)
    {
        if (string.IsNullOrWhiteSpace(rfidCode))
        {
            return null;
        }

        var vehicle = await _vehicleRepository.GetByPredicateAsync(v => v.RfiDcode == rfidCode);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return null;
        }

        var dto = _mapper.Map<VehicleReadDto>(vehicle);

        // Get assigned drivers
        var driverVehicles = await _driverVehicleRepository.GetAllByPredicateAsync(dv => dv.VehicleId == vehicle.Id);
        dto.AssignedDriverIds = driverVehicles
            .Select(dv => dv.DriverId)
            .ToList();

        return dto;
    }

    // NEW: Check if RFID code is available
    public async Task<bool> IsRfidCodeAvailableAsync(string rfidCode, string? excludeVehicleId = null)
    {
        if (string.IsNullOrWhiteSpace(rfidCode))
        {
            return true; // Empty/null RFID codes are allowed (optional field)
        }

        if (string.IsNullOrEmpty(excludeVehicleId))
        {
            // For create - check if RFID code exists at all
            var exists = await _vehicleRepository.ExistsByPredicateAsync(v => v.RfiDcode == rfidCode);
            return !exists; // Available if it doesn't exist
        }

        // For update - check if RFID code exists for a different vehicle
        var existsForOther = await _vehicleRepository.ExistsByPredicateAsync(v => 
            v.RfiDcode == rfidCode && v.Id != excludeVehicleId);
        return !existsForOther; // Available if it doesn't exist for another vehicle
    }

    public async Task<PagedResult<VehicleReadDto>> GetPagedVehiclesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        // Added RfiDcode to search properties
        var pagedResult = await _vehicleRepository.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: searchTerm,
            searchPredicate: searchTerm != null ? 
                (v => v.RegistrationNumber.Contains(searchTerm) || v.Model.Contains(searchTerm) || v.RfiDcode.Contains(searchTerm)) : 
                null,
            searchProperties: new[] { nameof(Vehicle.RegistrationNumber), nameof(Vehicle.Model), nameof(Vehicle.RfiDcode) }
        );

        var vehicleDtos = _mapper.Map<IEnumerable<VehicleReadDto>>(pagedResult.Items).ToList();

        // Get assigned drivers for all vehicles on this page
        var vehicleIds = vehicleDtos.Select(v => v.Id).ToList();
        var allDriverVehicles = await _driverVehicleRepository.GetAllByPredicateAsync(dv => vehicleIds.Contains(dv.VehicleId));

        // Group by vehicle ID and filter out deleted assignments
        var assignedDriversLookup = allDriverVehicles
            .Where(dv => !dv.IsDeleted)
            .GroupBy(dv => dv.VehicleId)
            .ToDictionary(g => g.Key, g => g.Select(dv => dv.DriverId).ToList());
            
        // Assign driver IDs to each vehicle DTO
        foreach (var dto in vehicleDtos)
        {
            dto.AssignedDriverIds = assignedDriversLookup.TryGetValue(dto.Id, out var driverIds) 
                ? driverIds 
                : new List<string>();
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
        if (!string.IsNullOrWhiteSpace(vehicle.Status))
            vehicle.Status = char.ToUpper(vehicle.Status[0]) + vehicle.Status.Substring(1).ToLower();
        var createdVehicle = await _vehicleRepository.CreateAsync(vehicle);
        
        return await GetByIdAsync(createdVehicle.Id) ?? 
            throw new InvalidOperationException("Failed to create vehicle");
    }

    public async Task<VehicleReadDto?> UpdateAsync(string id, UpdateVehicleDto dto)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { id });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return null;
        }

        // Check if registration number is available (excluding current vehicle)
        if (!await IsRegistrationNumberAvailableAsync(dto.RegistrationNumber, id))
        {
            throw new InvalidOperationException($"A different vehicle with registration number {dto.RegistrationNumber} already exists.");
        }

        // NEW: Validate RFID code uniqueness if provided and changed
        if (!string.IsNullOrWhiteSpace(dto.RfiDcode) && dto.RfiDcode != vehicle.RfiDcode)
        {
            var rfidAvailable = await IsRfidCodeAvailableAsync(dto.RfiDcode, id);
            if (!rfidAvailable)
            {
                throw new InvalidOperationException("This RFID code is already assigned to another vehicle.");
            }
        }

        _mapper.Map(dto, vehicle);
        if (!string.IsNullOrWhiteSpace(vehicle.Status))
            vehicle.Status = char.ToUpper(vehicle.Status[0]) + vehicle.Status.Substring(1).ToLower();

        // Repository will handle UpdatedAt and audit logging automatically
        var updatedVehicle = await _vehicleRepository.UpdateAsync(vehicle);
        return updatedVehicle == null ? null : await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { id });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        // Get all driver assignments for this vehicle
        var assignments = (await _driverVehicleRepository.GetAllByPredicateAsync(dv => dv.VehicleId == id)).ToList();

        // Delete all driver assignments - these will be automatically audited
        foreach (var assignment in assignments)
        {
            await _driverVehicleRepository.DeleteAsync(assignment.Id);
        }

        // Delete the vehicle - this will be automatically audited
        return await _vehicleRepository.DeleteAsync(id);
    }

    public async Task<bool> IsRegistrationNumberAvailableAsync(string registrationNumber, string? excludeVehicleId = null)
    {
        var result = await _vehicleRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            searchPredicate: v => 
                v.RegistrationNumber == registrationNumber && 
                (excludeVehicleId == null || v.Id != excludeVehicleId),
            searchProperties: new[] { nameof(Vehicle.RegistrationNumber) }
        );
        
        return !result.Items.Any();
    }
    
    public async Task<IEnumerable<string>> GetAssignedDriversAsync(string vehicleId)
    {
        if (string.IsNullOrEmpty(vehicleId))
        {
            return Enumerable.Empty<string>();
        }

        // Check if the vehicle exists and is not deleted using GetByIdsAsync
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { vehicleId });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            // Get all driver-vehicle assignments for this vehicle
            var assignments = (await _driverVehicleRepository.GetAllByPredicateAsync(dv => dv.VehicleId == vehicleId)).ToList();

            return assignments
                .Where(a => !string.IsNullOrEmpty(a.DriverId))
                .Select(a => a.DriverId)
                .ToList();
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            Console.WriteLine($"Error in GetAssignedDriversAsync: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> UpdateStatusAsync(string vehicleId, string status)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { vehicleId });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.Status = string.IsNullOrWhiteSpace(status) ? "Active"
            : char.ToUpper(status[0]) + status.Substring(1).ToLower();
        
        // Repository will handle UpdatedAt and audit logging automatically
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> AssignToSupplierAsync(string vehicleId, string? supplierId)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { vehicleId });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.SupplierId = supplierId;
        
        // Repository will handle UpdatedAt and audit logging automatically
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> AssignToTransporterAsync(string vehicleId, string? transporterId)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { vehicleId });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.TransporterId = transporterId;
        
        // Repository will handle UpdatedAt and audit logging automatically
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }

    public async Task<bool> AssignToOwnerAsync(string vehicleId, string? ownerId)
    {
        // Get the vehicle using GetByIdsAsync for consistency
        var vehicles = await _vehicleRepository.GetByIdsAsync(new[] { vehicleId });
        var vehicle = vehicles.FirstOrDefault();
        if (vehicle == null || vehicle.IsDeleted)
        {
            return false;
        }

        vehicle.OwnerId = ownerId;
        
        // Repository will handle UpdatedAt and audit logging automatically
        var result = await _vehicleRepository.UpdateAsync(vehicle);
        return result != null;
    }
    
    public async Task<IEnumerable<VehicleReadDto>> GetVehiclesBySupplierIdAsync(string supplierId)
    {
        // Get all vehicles by supplier ID using GetPagedAsync with a large page size
        var pagedResult = await _vehicleRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue, // Get all vehicles for this supplier
            searchPredicate: v => v.SupplierId == supplierId && !v.IsDeleted
        );

        var vehicles = pagedResult.Items.ToList();
        if (!vehicles.Any())
        {
            return Enumerable.Empty<VehicleReadDto>();
        }

        // Map to DTOs
        var vehicleDtos = _mapper.Map<IEnumerable<VehicleReadDto>>(vehicles).ToList();
    
        // Get all driver-vehicle relationships for these vehicles
        var vehicleIds = vehicleDtos.Select(v => v.Id).ToList();
        var driverVehiclesPaged = await _driverVehicleRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue, // Get all driver-vehicle relationships
            searchPredicate: dv => vehicleIds.Contains(dv.VehicleId) && !dv.IsDeleted
        );
    
        // Group by vehicle ID
        var driverVehiclesLookup = driverVehiclesPaged.Items
            .GroupBy(dv => dv.VehicleId)
            .ToDictionary(g => g.Key, g => g.Select(dv => dv.DriverId).ToList());
    
        // Assign driver IDs to each vehicle DTO
        foreach (var dto in vehicleDtos)
        {
            dto.AssignedDriverIds = driverVehiclesLookup.TryGetValue(dto.Id, out var driverIds) 
                ? driverIds 
                : new List<string>();
        }

        return vehicleDtos;
    }
}