using AutoMapper;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.Core.Services;

public class DriverService : IDriverService
{
    private readonly IRepository<Driver> _driverRepository;
    private readonly IRepository<DriverVehicle> _driverVehicleRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IMapper _mapper;
    private readonly IRepository<Transporter> _transporterRepository;

    public DriverService(
        IRepository<Driver> driverRepository,
        IRepository<DriverVehicle> driverVehicleRepository,
        IRepository<Supplier> supplierRepository,
        IRepository<Transporter> transporterRepository,
        IMapper mapper)
    {
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _driverVehicleRepository = driverVehicleRepository ?? throw new ArgumentNullException(nameof(driverVehicleRepository));
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        _transporterRepository = transporterRepository ?? throw new ArgumentNullException(nameof(transporterRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<DriverReadDto>> GetPagedDriversAsync(int pageNumber = 1, int pageSize = 10,
        string? searchTerm = null)
    {
        // Ensure page number and size are valid
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100); // Limit page size to 100 for performance

        // Get paginated drivers with search (added NfCcode to search properties)
        var pagedResult = await _driverRepository.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: searchTerm,
            searchProperties: new[] { nameof(Driver.FullName), nameof(Driver.LicenseNumber), nameof(Driver.NfCcode) }
        );

        if (!pagedResult.Items.Any())
        {
            return new PagedResult<DriverReadDto>
            {
                Items = Enumerable.Empty<DriverReadDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = 0
            };
        }

        // Map to DTOs
        var driverDtos = _mapper.Map<List<DriverReadDto>>(pagedResult.Items);
        var driverIds = driverDtos.Select(d => d.Id).ToList();

        // Get all vehicle assignments for the current page
        var allAssignments = new List<DriverVehicle>();
        foreach (var driverId in driverIds)
        {
            var driverAssignments = await _driverVehicleRepository.GetByIdsAsync(new[] { driverId });
            allAssignments.AddRange(driverAssignments);
        }

        // Group assignments by driver ID for efficient lookup
        var assignmentsByDriver = allAssignments
            .GroupBy(dv => dv.DriverId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.VehicleId).ToList());

        // Assign vehicle IDs to each driver
        foreach (var driverDto in driverDtos)
        {
            if (assignmentsByDriver.TryGetValue(driverDto.Id, out var vehicleIds))
            {
                driverDto.AssignedVehicleIds = vehicleIds;
            }
            else
            {
                driverDto.AssignedVehicleIds = new List<string>();
            }
        }

        return new PagedResult<DriverReadDto>
        {
            Items = driverDtos,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems
        };
    }
    

    public async Task<DriverReadDto?> GetByIdAsync(string id)
    {
        // Get the driver with basic info
        var drivers = await _driverRepository.GetByIdsAsync(new[] { id });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
            return null;

        // Get the assigned vehicles
        var assignedVehicles = await _driverVehicleRepository.GetByIdsAsync(new[] { id });

        var dto = _mapper.Map<DriverReadDto>(driver);
        dto.AssignedVehicleIds = assignedVehicles
            .Where(dv => dv.DriverId == id)
            .Select(dv => dv.VehicleId)
            .ToList();

        return dto;
    }

    // NEW: Get driver by NFC code
    public async Task<DriverReadDto?> GetByNfcCodeAsync(string nfcCode)
    {
        if (string.IsNullOrWhiteSpace(nfcCode))
        {
            return null;
        }

        var driver = await _driverRepository.GetByPredicateAsync(d => d.NfCcode == nfcCode);
        if (driver == null || driver.IsDeleted)
        {
            return null;
        }

        // Get the assigned vehicles
        var assignedVehicles = await _driverVehicleRepository.GetByIdsAsync(new[] { driver.Id });

        var dto = _mapper.Map<DriverReadDto>(driver);
        dto.AssignedVehicleIds = assignedVehicles
            .Where(dv => dv.DriverId == driver.Id)
            .Select(dv => dv.VehicleId)
            .ToList();

        return dto;
    }

    // NEW: Check if NFC code is available
    public async Task<bool> IsNfcCodeAvailableAsync(string nfcCode, string? excludeDriverId = null)
    {
        if (string.IsNullOrWhiteSpace(nfcCode))
        {
            return true; // Empty/null NFC codes are allowed (optional field)
        }

        if (string.IsNullOrEmpty(excludeDriverId))
        {
            // For create - check if NFC code exists at all
            var exists = await _driverRepository.ExistsByPredicateAsync(d => d.NfCcode == nfcCode);
            return !exists; // Available if it doesn't exist
        }

        // For update - check if NFC code exists for a different driver
        var existsForOther = await _driverRepository.ExistsByPredicateAsync(d => 
            d.NfCcode == nfcCode && d.Id != excludeDriverId);
        return !existsForOther; // Available if it doesn't exist for another driver
    }

    public async Task<DriverReadDto> CreateAsync(CreateDriverDto dto)
    {
        
        // Map DTO to entity
        var driver = _mapper.Map<Driver>(dto);
        
        // Set default status if not provided
        if (string.IsNullOrWhiteSpace(driver.Status))
        {
            driver.Status = "active";
        }

        // Create the driver (NFC code will be added later via update)
        var createdDriver = await _driverRepository.CreateAsync(driver);
        
        if (createdDriver == null)
        {
            throw new InvalidOperationException("Failed to create driver.");
        }

        // Return the created driver with its ID
        return await GetByIdAsync(createdDriver.Id);
    }

    public async Task<DriverReadDto?> UpdateAsync(string id, UpdateDriverDto dto)
    {
        // Validate input
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Driver ID is required", nameof(id));
        }

        // Get the existing driver
        var drivers = await _driverRepository.GetByIdsAsync(new[] { id });
        var existingDriver = drivers.FirstOrDefault();

        if (existingDriver == null || existingDriver.IsDeleted)
        {
            return null;
        }

        // Check if license number is being updated and if it's already in use
        if (!string.IsNullOrEmpty(dto.LicenseNumber) &&
            !string.Equals(existingDriver.LicenseNumber, dto.LicenseNumber, StringComparison.OrdinalIgnoreCase))
        {
            var licenseExists = await _driverRepository.ExistsByPredicateAsync(d =>
                d.LicenseNumber == dto.LicenseNumber && d.Id != id);
            if (licenseExists)
                throw new InvalidOperationException("A driver with this license number already exists.");
        }

        // NEW: Validate NFC code uniqueness if provided and changed
        if (!string.IsNullOrWhiteSpace(dto.NfCcode) && dto.NfCcode != existingDriver.NfCcode)
        {
            var nfcAvailable = await IsNfcCodeAvailableAsync(dto.NfCcode, id);
            if (!nfcAvailable)
            {
                throw new InvalidOperationException("This NFC code is already assigned to another driver.");
            }
        }

        // Map DTO to existing entity
        _mapper.Map(dto, existingDriver);
        existingDriver.UpdatedAt = DateTime.UtcNow;

        // Update the driver
        var updatedDriver = await _driverRepository.UpdateAsync(existingDriver);
        if (updatedDriver == null)
        {
            throw new InvalidOperationException("Failed to update driver.");
        }

        // Return the updated driver
        return await GetByIdAsync(updatedDriver.Id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // Get the driver using GetByIdsAsync
        var drivers = await _driverRepository.GetByIdsAsync(new[] { id });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Remove all vehicle assignments for this driver using GetByIdsAsync
        var assignments = await _driverVehicleRepository.GetByIdsAsync(new[] { id });
        var driverAssignments = assignments.Where(dv => dv.DriverId == id).ToList();
        
        foreach (var assignment in driverAssignments)
        {
            await _driverVehicleRepository.DeleteAsync(assignment.Id);
        }

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

        // Get all assignments for this driver
        var driverAssignments = await _driverVehicleRepository.GetByIdsAsync(new[] { driverId });
        
        // Check if this vehicle is already assigned to the driver
        var isAlreadyAssigned = driverAssignments
            .Any(dv => dv.DriverId == driverId && dv.VehicleId == vehicleId);
            
        if (isAlreadyAssigned)
        {
            return; // Already assigned, idempotent operation
        }

        // Create new assignment
        await _driverVehicleRepository.CreateAsync(new DriverVehicle
        {
            DriverId = driverId,
            VehicleId = vehicleId,
            AssignedDate = DateTime.UtcNow
        });
    }

    public async Task<bool> RemoveVehicleAsync(string driverId, string vehicleId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(vehicleId))
        {
            return false;
        }

        // Check if the driver exists and is not deleted using GetByIdsAsync
        var drivers = await _driverRepository.GetByIdsAsync(new[] { driverId });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Find the assignment using GetByIdsAsync
        var assignments = await _driverVehicleRepository.GetByIdsAsync(new[] { driverId, vehicleId });
        var assignment = assignments
            .FirstOrDefault(dv => dv.DriverId == driverId && dv.VehicleId == vehicleId);

        if (assignment == null)
        {
            return true; // Not assigned, consider it success (idempotent)
        }

        // Remove the assignment
        await _driverVehicleRepository.DeleteAsync(assignment.Id);
        return true;
    }
    
    //driver supplier methods adding and removing 
    public async Task<bool> AssignToSupplierAsync(string driverId, string supplierId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(supplierId))
        {
            return false;
        }

        // Check if the driver exists and is not deleted using GetByIdsAsync
        var drivers = await _driverRepository.GetByIdsAsync(new[] { driverId });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Check if the supplier exists and is not deleted using GetByIdsAsync
        var suppliers = await _supplierRepository.GetByIdsAsync(new[] { supplierId });
        var supplier = suppliers.FirstOrDefault();
        if (supplier == null || supplier.IsDeleted)
        {
            return false;
        }

        // Check if the driver is already assigned to this supplier
        if (driver.SupplierId == supplierId)
        {
            return true; // Already assigned to this supplier
        }

        // Check if the driver is assigned to a different supplier
        if (!string.IsNullOrEmpty(driver.SupplierId) && driver.SupplierId != supplierId)
        {
            // Get the current supplier's name for a better error message
            var currentSuppliers = await _supplierRepository.GetByIdsAsync(new[] { driver.SupplierId });
            var currentSupplier = currentSuppliers.FirstOrDefault();
            var currentSupplierName = currentSupplier?.Name ?? "Unknown Supplier";
            
            // Get the target supplier's name for a better error message
            var targetSupplierName = supplier.Name ?? "Unknown Supplier";
            
            // Throw a meaningful exception with details
            throw new InvalidOperationException(
                $"Driver is already assigned to supplier '{currentSupplierName}'. " +
                $"Please unassign from the current supplier before assigning to '{targetSupplierName}'.");
        }

        driver.SupplierId = supplierId;
        var result = await _driverRepository.UpdateAsync(driver);
        return result != null;
    }
    
    //remove a driver form a supplier
    public async Task<bool> RemoveFromSupplierAsync(string driverId, string supplierId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(supplierId))
        {
            return false;
        }

        // Check if the driver exists and is not deleted using GetByIdsAsync
        var drivers = await _driverRepository.GetByIdsAsync(new[] { driverId });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Check if the supplier exists and is not deleted using GetByIdsAsync
        var suppliers = await _supplierRepository.GetByIdsAsync(new[] { supplierId });
        var supplier = suppliers.FirstOrDefault();
        if (supplier == null || supplier.IsDeleted)
        {
            return false;
        }

        // Check if the driver is assigned to this supplier
        if (driver.SupplierId != supplierId)
        {
            return true; // Not assigned to this supplier
        }

        // Remove the driver from the supplier
        driver.SupplierId = null;
        var result = await _driverRepository.UpdateAsync(driver);
        return result != null;
    }
    
    // Assign driver to a transporter
    public async Task<bool> AssignToTransporterAsync(string driverId, string transporterId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(transporterId))
        {
            return false;
        }

        // Check if the driver exists and is not deleted using GetByIdsAsync
        var drivers = await _driverRepository.GetByIdsAsync(new[] { driverId });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        // Check if the transporter exists and is not deleted using GetByIdsAsync
        var transporters = await _transporterRepository.GetByIdsAsync(new[] { transporterId });
        var transporter = transporters.FirstOrDefault();
        if (transporter == null || transporter.IsDeleted)
        {
            return false;
        }

        // Check if the driver is already assigned to this transporter
        if (driver.TransporterId == transporterId)
        {
            return true; // Already assigned to this transporter
        }

        // Check if the driver is assigned to a different transporter
        if (!string.IsNullOrEmpty(driver.TransporterId) && driver.TransporterId != transporterId)
        {
            // Get the current transporter's name for a better error message
            var currentTransporters = await _transporterRepository.GetByIdsAsync(new[] { driver.TransporterId });
            var currentTransporter = currentTransporters.FirstOrDefault();
            var currentTransporterName = currentTransporter?.Name ?? "Unknown Transporter";
            
            // Get the target transporter's name for a better error message
            var targetTransporterName = transporter.Name ?? "Unknown Transporter";
            
            // Throw a meaningful exception with details
            throw new InvalidOperationException(
                $"Driver is already assigned to transporter '{currentTransporterName}'. " +
                $"Please unassign from the current transporter before assigning to '{targetTransporterName}'.");
        }

        driver.TransporterId = transporterId;
        var result = await _driverRepository.UpdateAsync(driver);
        return result != null;
    }

    // Remove a driver from a transporter
    public async Task<bool> RemoveFromTransporterAsync(string driverId, string transporterId)
    {
        if (string.IsNullOrEmpty(driverId) || string.IsNullOrEmpty(transporterId))
        {
            return false;
        }

        // Check if the driver exists and is not deleted using GetByIdsAsync
        var drivers = await _driverRepository.GetByIdsAsync(new[] { driverId });
        var driver = drivers.FirstOrDefault();
        if (driver == null || driver.IsDeleted)
        {
            return false;
        }

        var transporters = await _transporterRepository.GetByIdsAsync(new[] { transporterId });
        var transporter = transporters.FirstOrDefault();
        if (transporter == null || transporter.IsDeleted)
        {
            return false;
        }

        // Check if the driver is assigned to this transporter
        if (driver.TransporterId != transporterId)
        {
            return true; // Not assigned to this transporter
        }

        // Remove the driver from the transporter
        driver.TransporterId = null;
        var result = await _driverRepository.UpdateAsync(driver);
        return result != null;
    }
}