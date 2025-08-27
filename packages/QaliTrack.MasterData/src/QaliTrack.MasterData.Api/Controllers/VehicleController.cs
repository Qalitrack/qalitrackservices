using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Core.Modules.Vehicle.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Vehicle Module")]
public class VehicleController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public VehicleController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get vehicles with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of vehicles</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleSummaryDto>>>> GetVehicles(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Vehicles
                .Include(v => v.VehicleType)
                .Select(v => new VehicleSummaryDto
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    FuelType = v.FuelType,
                    VehicleTypeName = v.VehicleType.Name,
                    Status = v.Status,
                    CurrentMileage = v.CurrentMileage,
                    NextInspectionDue = v.NextInspectionDue,
                    InsuranceExpiryDate = v.InsuranceExpiryDate,
                    CreatedAt = v.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleSummaryDto>>.ErrorResponse("Error retrieving vehicles", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDetailDto>>> GetVehicle(Guid id)
    {
        try
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Registration)
                .Include(v => v.Specification)
                .Where(v => v.Id == id)
                .Select(v => new VehicleDetailDto
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    VIN = v.VIN,
                    EngineNumber = v.EngineNumber,
                    FuelType = v.FuelType,
                    MaxWeight = v.MaxWeight,
                    TareWeight = v.TareWeight,
                    PayloadCapacity = v.PayloadCapacity,
                    Status = v.Status,
                    LastInspectionDate = v.LastInspectionDate,
                    NextInspectionDue = v.NextInspectionDue,
                    InsuranceExpiryDate = v.InsuranceExpiryDate,
                    RegistrationExpiryDate = v.RegistrationExpiryDate,
                    CurrentMileage = v.CurrentMileage,
                    OrganizationId = v.OrganizationId,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleTypeName = v.VehicleType.Name,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    HasRegistration = v.Registration != null,
                    HasInsurance = v.InsurancePolicies.Any(i => i.Status == "Active"),
                    HasSpecification = v.Specification != null,
                    InspectionDue = v.NextInspectionDue.HasValue && v.NextInspectionDue < DateTime.Today.AddDays(30)
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound(ApiResponse<VehicleDetailDto>.ErrorResponse("Vehicle not found"));
            }

            return Ok(ApiResponse<VehicleDetailDto>.SuccessResponse(vehicle));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDetailDto>.ErrorResponse("Error retrieving vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Create new vehicle
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VehicleDetailDto>>> CreateVehicle(CreateVehicleDto dto)
    {
        try
        {
            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = dto.RegistrationNumber,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Color = dto.Color,
                VIN = dto.VIN,
                EngineNumber = dto.EngineNumber ?? string.Empty,
                FuelType = dto.FuelType,
                MaxWeight = dto.MaxWeight ?? 0,
                TareWeight = dto.TareWeight ?? 0,
                PayloadCapacity = dto.PayloadCapacity ?? 0,
                CurrentMileage = dto.CurrentMileage,
                LastInspectionDate = dto.LastInspectionDate,
                InsuranceExpiryDate = dto.InsuranceExpiryDate,
                RegistrationExpiryDate = dto.RegistrationExpiryDate,
                VehicleTypeId = dto.VehicleTypeId,
                OrganizationId = dto.OrganizationId,
                Status = "Active",
                Notes = dto.Notes
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Return detailed response
            var responseDto = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Where(v => v.Id == vehicle.Id)
                .Select(v => new VehicleDetailDto
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    VIN = v.VIN,
                    EngineNumber = v.EngineNumber,
                    FuelType = v.FuelType,
                    MaxWeight = v.MaxWeight,
                    TareWeight = v.TareWeight,
                    PayloadCapacity = v.PayloadCapacity,
                    Status = v.Status,
                    LastInspectionDate = v.LastInspectionDate,
                    NextInspectionDue = v.NextInspectionDue,
                    InsuranceExpiryDate = v.InsuranceExpiryDate,
                    RegistrationExpiryDate = v.RegistrationExpiryDate,
                    CurrentMileage = v.CurrentMileage,
                    OrganizationId = v.OrganizationId,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleTypeName = v.VehicleType.Name,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    HasRegistration = false,
                    HasInsurance = false,
                    HasSpecification = false,
                    InspectionDue = false
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetVehicle), 
                new { id = vehicle.Id }, 
                ApiResponse<VehicleDetailDto>.SuccessResponse(responseDto!, "Vehicle created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDetailDto>.ErrorResponse("Error creating vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDetailDto>>> UpdateVehicle(Guid id, UpdateVehicleDto dto)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(ApiResponse<VehicleDetailDto>.ErrorResponse("Vehicle not found"));
            }

            // Update properties from DTO
            vehicle.RegistrationNumber = dto.RegistrationNumber;
            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.Color = dto.Color;
            vehicle.FuelType = dto.FuelType;
            vehicle.Status = dto.Status;
            vehicle.CurrentMileage = dto.CurrentMileage;
            vehicle.EngineNumber = dto.EngineNumber ?? vehicle.EngineNumber;
            vehicle.MaxWeight = dto.MaxWeight ?? vehicle.MaxWeight;
            vehicle.TareWeight = dto.TareWeight ?? vehicle.TareWeight;
            vehicle.PayloadCapacity = dto.PayloadCapacity ?? vehicle.PayloadCapacity;
            vehicle.LastInspectionDate = dto.LastInspectionDate;
            vehicle.NextInspectionDue = dto.NextInspectionDue;
            vehicle.InsuranceExpiryDate = dto.InsuranceExpiryDate;
            vehicle.RegistrationExpiryDate = dto.RegistrationExpiryDate;
            vehicle.Notes = dto.Notes;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated vehicle details
            var responseDto = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Registration)
                .Include(v => v.Specification)
                .Include(v => v.InsurancePolicies)
                .Where(v => v.Id == id)
                .Select(v => new VehicleDetailDto
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    VIN = v.VIN,
                    EngineNumber = v.EngineNumber,
                    FuelType = v.FuelType,
                    MaxWeight = v.MaxWeight,
                    TareWeight = v.TareWeight,
                    PayloadCapacity = v.PayloadCapacity,
                    Status = v.Status,
                    LastInspectionDate = v.LastInspectionDate,
                    NextInspectionDue = v.NextInspectionDue,
                    InsuranceExpiryDate = v.InsuranceExpiryDate,
                    RegistrationExpiryDate = v.RegistrationExpiryDate,
                    CurrentMileage = v.CurrentMileage,
                    OrganizationId = v.OrganizationId,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleTypeName = v.VehicleType.Name,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    HasRegistration = v.Registration != null,
                    HasInsurance = v.InsurancePolicies.Any(i => i.Status == "Active"),
                    HasSpecification = v.Specification != null,
                    InspectionDue = v.NextInspectionDue.HasValue && v.NextInspectionDue < DateTime.Today.AddDays(30)
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<VehicleDetailDto>.SuccessResponse(responseDto!, "Vehicle updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDetailDto>.ErrorResponse("Error updating vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicle(Guid id)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle not found"));
            }

            vehicle.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDetailDto>>> PatchVehicle(Guid id, PatchVehicleDto dto)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(ApiResponse<VehicleDetailDto>.ErrorResponse("Vehicle not found"));
            }

            if (dto.RegistrationNumber != null) vehicle.RegistrationNumber = dto.RegistrationNumber;
            if (dto.Make != null) vehicle.Make = dto.Make;
            if (dto.Model != null) vehicle.Model = dto.Model;
            if (dto.Year.HasValue) vehicle.Year = dto.Year.Value;
            if (dto.Color != null) vehicle.Color = dto.Color;
            if (dto.FuelType != null) vehicle.FuelType = dto.FuelType;
            if (dto.Status != null) vehicle.Status = dto.Status;
            if (dto.CurrentMileage.HasValue) vehicle.CurrentMileage = dto.CurrentMileage;
            if (dto.EngineNumber != null) vehicle.EngineNumber = dto.EngineNumber;
            if (dto.MaxWeight.HasValue) vehicle.MaxWeight = dto.MaxWeight.Value;
            if (dto.TareWeight.HasValue) vehicle.TareWeight = dto.TareWeight.Value;
            if (dto.PayloadCapacity.HasValue) vehicle.PayloadCapacity = dto.PayloadCapacity.Value;
            if (dto.LastInspectionDate.HasValue) vehicle.LastInspectionDate = dto.LastInspectionDate;
            if (dto.NextInspectionDue.HasValue) vehicle.NextInspectionDue = dto.NextInspectionDue;
            if (dto.InsuranceExpiryDate.HasValue) vehicle.InsuranceExpiryDate = dto.InsuranceExpiryDate;
            if (dto.RegistrationExpiryDate.HasValue) vehicle.RegistrationExpiryDate = dto.RegistrationExpiryDate;
            if (dto.Notes != null) vehicle.Notes = dto.Notes;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Registration)
                .Include(v => v.Specification)
                .Include(v => v.InsurancePolicies)
                .Where(v => v.Id == id)
                .Select(v => new VehicleDetailDto
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    VIN = v.VIN,
                    EngineNumber = v.EngineNumber,
                    FuelType = v.FuelType,
                    MaxWeight = v.MaxWeight,
                    TareWeight = v.TareWeight,
                    PayloadCapacity = v.PayloadCapacity,
                    Status = v.Status,
                    LastInspectionDate = v.LastInspectionDate,
                    NextInspectionDue = v.NextInspectionDue,
                    InsuranceExpiryDate = v.InsuranceExpiryDate,
                    RegistrationExpiryDate = v.RegistrationExpiryDate,
                    CurrentMileage = v.CurrentMileage,
                    OrganizationId = v.OrganizationId,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleTypeName = v.VehicleType.Name,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    HasRegistration = v.Registration != null,
                    HasInsurance = v.InsurancePolicies.Any(i => i.Status == "Active"),
                    HasSpecification = v.Specification != null,
                    InspectionDue = v.NextInspectionDue.HasValue && v.NextInspectionDue < DateTime.Today.AddDays(30)
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<VehicleDetailDto>.SuccessResponse(responseDto!, "Vehicle updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDetailDto>.ErrorResponse("Error updating vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle registration details
    /// </summary>
    [HttpGet("{id}/registration")]
    public async Task<ActionResult<ApiResponse<VehicleRegistrationDto>>> GetVehicleRegistration(Guid id)
    {
        try
        {
            var registration = await _context.VehicleRegistrations
                .Where(r => r.VehicleId == id)
                .Select(r => new VehicleRegistrationDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    RegistrationNumber = r.RegistrationNumber,
                    RegistrationDate = r.RegistrationDate,
                    ExpiryDate = r.ExpiryDate,
                    IssuingAuthority = r.IssuingAuthority,
                    Status = r.Status,
                    RegistrationFee = r.RegistrationFee,
                    LastRenewalDate = r.LastRenewalDate,
                    RenewalStatus = r.RenewalStatus,
                    IsExpired = r.ExpiryDate < DateTime.Today,
                    DaysUntilExpiry = (int)(r.ExpiryDate - DateTime.Today).TotalDays,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (registration == null)
            {
                return NotFound(ApiResponse<VehicleRegistrationDto>.ErrorResponse("Vehicle registration not found"));
            }

            return Ok(ApiResponse<VehicleRegistrationDto>.SuccessResponse(registration));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleRegistrationDto>.ErrorResponse("Error retrieving vehicle registration", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle registration
    /// </summary>
    [HttpPut("{id}/registration")]
    public async Task<ActionResult<ApiResponse<VehicleRegistrationDto>>> UpdateVehicleRegistration(Guid id, UpdateVehicleRegistrationDto dto)
    {
        try
        {
            var registration = await _context.VehicleRegistrations
                .FirstOrDefaultAsync(r => r.VehicleId == id);

            if (registration == null)
            {
                return NotFound(ApiResponse<VehicleRegistrationDto>.ErrorResponse("Vehicle registration not found"));
            }

            registration.RegistrationNumber = dto.RegistrationNumber;
            registration.RegistrationDate = dto.RegistrationDate;
            registration.ExpiryDate = dto.ExpiryDate;
            registration.IssuingAuthority = dto.IssuingAuthority;
            registration.Status = dto.Status;
            registration.RegistrationFee = dto.RegistrationFee;
            registration.Notes = dto.Notes ?? string.Empty;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleRegistrationDto
            {
                Id = registration.Id,
                VehicleId = registration.VehicleId,
                RegistrationNumber = registration.RegistrationNumber,
                RegistrationDate = registration.RegistrationDate,
                ExpiryDate = registration.ExpiryDate,
                IssuingAuthority = registration.IssuingAuthority,
                Status = registration.Status,
                RegistrationFee = registration.RegistrationFee,
                LastRenewalDate = registration.LastRenewalDate,
                RenewalStatus = registration.RenewalStatus,
                IsExpired = registration.ExpiryDate < DateTime.Today,
                DaysUntilExpiry = (int)(registration.ExpiryDate - DateTime.Today).TotalDays,
                Notes = registration.Notes,
                CreatedAt = registration.CreatedAt
            };

            return Ok(ApiResponse<VehicleRegistrationDto>.SuccessResponse(responseDto, "Vehicle registration updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleRegistrationDto>.ErrorResponse("Error updating vehicle registration", ex.Message));
        }
    }

    /// <summary>
    /// Create or update vehicle registration
    /// </summary>
    [HttpPost("{id}/registration")]
    public async Task<ActionResult<ApiResponse<VehicleRegistrationDto>>> CreateVehicleRegistration(Guid id, CreateVehicleRegistrationDto dto)
    {
        try
        {
            var registration = new VehicleRegistration
            {
                Id = Guid.NewGuid(),
                VehicleId = id,
                RegistrationNumber = dto.RegistrationNumber,
                RegistrationDate = dto.RegistrationDate,
                ExpiryDate = dto.ExpiryDate,
                IssuingAuthority = dto.IssuingAuthority,
                Status = dto.Status,
                RegistrationFee = dto.RegistrationFee
            };

            _context.VehicleRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleRegistrationDto
            {
                Id = registration.Id,
                VehicleId = registration.VehicleId,
                RegistrationNumber = registration.RegistrationNumber,
                RegistrationDate = registration.RegistrationDate,
                ExpiryDate = registration.ExpiryDate,
                IssuingAuthority = registration.IssuingAuthority,
                Status = registration.Status,
                RegistrationFee = registration.RegistrationFee,
                IsExpired = registration.ExpiryDate < DateTime.Today,
                DaysUntilExpiry = (int)(registration.ExpiryDate - DateTime.Today).TotalDays,
                CreatedAt = registration.CreatedAt
            };

            return Ok(ApiResponse<VehicleRegistrationDto>.SuccessResponse(responseDto, "Vehicle registration created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleRegistrationDto>.ErrorResponse("Error creating vehicle registration", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle registration
    /// </summary>
    [HttpPatch("{id}/registration")]
    public async Task<ActionResult<ApiResponse<VehicleRegistrationDto>>> PatchVehicleRegistration(Guid id, PatchVehicleRegistrationDto dto)
    {
        try
        {
            var registration = await _context.VehicleRegistrations
                .FirstOrDefaultAsync(r => r.VehicleId == id);

            if (registration == null)
            {
                return NotFound(ApiResponse<VehicleRegistrationDto>.ErrorResponse("Vehicle registration not found"));
            }

            if (dto.RegistrationNumber != null) registration.RegistrationNumber = dto.RegistrationNumber;
            if (dto.RegistrationDate.HasValue) registration.RegistrationDate = dto.RegistrationDate.Value;
            if (dto.ExpiryDate.HasValue) registration.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.IssuingAuthority != null) registration.IssuingAuthority = dto.IssuingAuthority;
            if (dto.Status != null) registration.Status = dto.Status;
            if (dto.RegistrationFee.HasValue) registration.RegistrationFee = dto.RegistrationFee;
            if (dto.Notes != null) registration.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleRegistrationDto
            {
                Id = registration.Id,
                VehicleId = registration.VehicleId,
                RegistrationNumber = registration.RegistrationNumber,
                RegistrationDate = registration.RegistrationDate,
                ExpiryDate = registration.ExpiryDate,
                IssuingAuthority = registration.IssuingAuthority,
                Status = registration.Status,
                RegistrationFee = registration.RegistrationFee,
                LastRenewalDate = registration.LastRenewalDate,
                RenewalStatus = registration.RenewalStatus,
                IsExpired = registration.ExpiryDate < DateTime.Today,
                DaysUntilExpiry = (int)(registration.ExpiryDate - DateTime.Today).TotalDays,
                Notes = registration.Notes,
                CreatedAt = registration.CreatedAt
            };

            return Ok(ApiResponse<VehicleRegistrationDto>.SuccessResponse(responseDto, "Vehicle registration updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleRegistrationDto>.ErrorResponse("Error updating vehicle registration", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle registration
    /// </summary>
    [HttpDelete("{id}/registration")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleRegistration(Guid id)
    {
        try
        {
            var registration = await _context.VehicleRegistrations
                .FirstOrDefaultAsync(r => r.VehicleId == id);

            if (registration == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle registration not found"));
            }

            _context.VehicleRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle registration deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle registration", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle specification
    /// </summary>
    [HttpGet("{id}/specification")]
    public async Task<ActionResult<ApiResponse<VehicleSpecificationDto>>> GetVehicleSpecification(Guid id)
    {
        try
        {
            var specification = await _context.VehicleSpecifications
                .Where(s => s.VehicleId == id)
                .Select(s => new VehicleSpecificationDto
                {
                    Id = s.Id,
                    VehicleId = s.VehicleId,
                    Length = s.Length,
                    Width = s.Width,
                    Height = s.Height,
                    Wheelbase = s.Wheelbase,
                    GroundClearance = s.GroundClearance,
                    TransmissionType = s.TransmissionType,
                    NumberOfGears = s.NumberOfGears,
                    DriveType = s.DriveType,
                    EngineCapacity = s.EngineCapacity,
                    EnginePower = s.EnginePower,
                    EngineTorque = s.EngineTorque,
                    FuelTankCapacity = s.FuelTankCapacity,
                    FuelConsumption = s.FuelConsumption,
                    EmissionStandard = s.EmissionStandard,
                    SeatingCapacity = s.SeatingCapacity,
                    SpecialFeatures = s.SpecialFeatures,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (specification == null)
            {
                return NotFound(ApiResponse<VehicleSpecificationDto>.ErrorResponse("Vehicle specification not found"));
            }

            return Ok(ApiResponse<VehicleSpecificationDto>.SuccessResponse(specification));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleSpecificationDto>.ErrorResponse("Error retrieving vehicle specification", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle specification
    /// </summary>
    [HttpPut("{id}/specification")]
    public async Task<ActionResult<ApiResponse<VehicleSpecificationDto>>> UpdateVehicleSpecification(Guid id, UpdateVehicleSpecificationDto dto)
    {
        try
        {
            var specification = await _context.VehicleSpecifications
                .FirstOrDefaultAsync(s => s.VehicleId == id);

            if (specification == null)
            {
                return NotFound(ApiResponse<VehicleSpecificationDto>.ErrorResponse("Vehicle specification not found"));
            }

            specification.Length = dto.Length;
            specification.Width = dto.Width;
            specification.Height = dto.Height;
            specification.EngineCapacity = dto.EngineCapacity;
            specification.EnginePower = dto.EnginePower;
            specification.FuelTankCapacity = dto.FuelTankCapacity;
            specification.SeatingCapacity = dto.SeatingCapacity;
            specification.TransmissionType = dto.TransmissionType;
            specification.DriveType = dto.DriveType;
            specification.Wheelbase = dto.Wheelbase ?? specification.Wheelbase;
            specification.GroundClearance = dto.GroundClearance ?? specification.GroundClearance;
            specification.NumberOfGears = dto.NumberOfGears ?? specification.NumberOfGears;
            specification.EngineTorque = dto.EngineTorque ?? specification.EngineTorque;
            specification.FuelConsumption = dto.FuelConsumption ?? specification.FuelConsumption;
            specification.EmissionStandard = dto.EmissionStandard ?? specification.EmissionStandard;
            specification.SpecialFeatures = dto.SpecialFeatures;
            specification.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleSpecificationDto
            {
                Id = specification.Id,
                VehicleId = specification.VehicleId,
                Length = specification.Length,
                Width = specification.Width,
                Height = specification.Height,
                Wheelbase = specification.Wheelbase,
                GroundClearance = specification.GroundClearance,
                TransmissionType = specification.TransmissionType,
                NumberOfGears = specification.NumberOfGears,
                DriveType = specification.DriveType,
                EngineCapacity = specification.EngineCapacity,
                EnginePower = specification.EnginePower,
                EngineTorque = specification.EngineTorque,
                FuelTankCapacity = specification.FuelTankCapacity,
                FuelConsumption = specification.FuelConsumption,
                EmissionStandard = specification.EmissionStandard,
                SeatingCapacity = specification.SeatingCapacity,
                SpecialFeatures = specification.SpecialFeatures,
                Notes = specification.Notes,
                CreatedAt = specification.CreatedAt
            };

            return Ok(ApiResponse<VehicleSpecificationDto>.SuccessResponse(responseDto, "Vehicle specification updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleSpecificationDto>.ErrorResponse("Error updating vehicle specification", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle specification
    /// </summary>
    [HttpPost("{id}/specification")]
    public async Task<ActionResult<ApiResponse<VehicleSpecificationDto>>> CreateVehicleSpecification(Guid id, CreateVehicleSpecificationDto dto)
    {
        try
        {
            var specification = new VehicleSpecification
            {
                Id = Guid.NewGuid(),
                VehicleId = id,
                Length = dto.Length,
                Width = dto.Width,
                Height = dto.Height,
                EngineCapacity = dto.EngineCapacity,
                EnginePower = dto.EnginePower,
                FuelTankCapacity = dto.FuelTankCapacity,
                SeatingCapacity = dto.SeatingCapacity,
                TransmissionType = dto.TransmissionType,
                DriveType = dto.DriveType
            };

            _context.VehicleSpecifications.Add(specification);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleSpecificationDto
            {
                Id = specification.Id,
                VehicleId = specification.VehicleId,
                Length = specification.Length,
                Width = specification.Width,
                Height = specification.Height,
                Wheelbase = specification.Wheelbase,
                GroundClearance = specification.GroundClearance,
                TransmissionType = specification.TransmissionType,
                NumberOfGears = specification.NumberOfGears,
                DriveType = specification.DriveType,
                EngineCapacity = specification.EngineCapacity,
                EnginePower = specification.EnginePower,
                EngineTorque = specification.EngineTorque,
                FuelTankCapacity = specification.FuelTankCapacity,
                FuelConsumption = specification.FuelConsumption,
                EmissionStandard = specification.EmissionStandard,
                SeatingCapacity = specification.SeatingCapacity,
                SpecialFeatures = specification.SpecialFeatures,
                Notes = specification.Notes,
                CreatedAt = specification.CreatedAt
            };

            return Ok(ApiResponse<VehicleSpecificationDto>.SuccessResponse(responseDto, "Vehicle specification created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleSpecificationDto>.ErrorResponse("Error creating vehicle specification", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle specification
    /// </summary>
    [HttpPatch("{id}/specification")]
    public async Task<ActionResult<ApiResponse<VehicleSpecificationDto>>> PatchVehicleSpecification(Guid id, PatchVehicleSpecificationDto dto)
    {
        try
        {
            var specification = await _context.VehicleSpecifications
                .FirstOrDefaultAsync(s => s.VehicleId == id);

            if (specification == null)
            {
                return NotFound(ApiResponse<VehicleSpecificationDto>.ErrorResponse("Vehicle specification not found"));
            }

            if (dto.Length.HasValue) specification.Length = dto.Length.Value;
            if (dto.Width.HasValue) specification.Width = dto.Width.Value;
            if (dto.Height.HasValue) specification.Height = dto.Height.Value;
            if (dto.EngineCapacity.HasValue) specification.EngineCapacity = dto.EngineCapacity.Value;
            if (dto.EnginePower.HasValue) specification.EnginePower = dto.EnginePower.Value;
            if (dto.FuelTankCapacity.HasValue) specification.FuelTankCapacity = dto.FuelTankCapacity.Value;
            if (dto.SeatingCapacity.HasValue) specification.SeatingCapacity = dto.SeatingCapacity.Value;
            if (dto.TransmissionType != null) specification.TransmissionType = dto.TransmissionType;
            if (dto.DriveType != null) specification.DriveType = dto.DriveType;
            if (dto.Wheelbase.HasValue) specification.Wheelbase = dto.Wheelbase.Value;
            if (dto.GroundClearance.HasValue) specification.GroundClearance = dto.GroundClearance.Value;
            if (dto.NumberOfGears.HasValue) specification.NumberOfGears = dto.NumberOfGears.Value;
            if (dto.EngineTorque.HasValue) specification.EngineTorque = dto.EngineTorque.Value;
            if (dto.FuelConsumption.HasValue) specification.FuelConsumption = dto.FuelConsumption.Value;
            if (dto.EmissionStandard != null) specification.EmissionStandard = dto.EmissionStandard;
            if (dto.SpecialFeatures != null) specification.SpecialFeatures = dto.SpecialFeatures;
            if (dto.Notes != null) specification.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleSpecificationDto
            {
                Id = specification.Id,
                VehicleId = specification.VehicleId,
                Length = specification.Length,
                Width = specification.Width,
                Height = specification.Height,
                Wheelbase = specification.Wheelbase,
                GroundClearance = specification.GroundClearance,
                TransmissionType = specification.TransmissionType,
                NumberOfGears = specification.NumberOfGears,
                DriveType = specification.DriveType,
                EngineCapacity = specification.EngineCapacity,
                EnginePower = specification.EnginePower,
                EngineTorque = specification.EngineTorque,
                FuelTankCapacity = specification.FuelTankCapacity,
                FuelConsumption = specification.FuelConsumption,
                EmissionStandard = specification.EmissionStandard,
                SeatingCapacity = specification.SeatingCapacity,
                SpecialFeatures = specification.SpecialFeatures,
                Notes = specification.Notes,
                CreatedAt = specification.CreatedAt
            };

            return Ok(ApiResponse<VehicleSpecificationDto>.SuccessResponse(responseDto, "Vehicle specification updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleSpecificationDto>.ErrorResponse("Error updating vehicle specification", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle specification
    /// </summary>
    [HttpDelete("{id}/specification")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleSpecification(Guid id)
    {
        try
        {
            var specification = await _context.VehicleSpecifications
                .FirstOrDefaultAsync(s => s.VehicleId == id);

            if (specification == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle specification not found"));
            }

            _context.VehicleSpecifications.Remove(specification);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle specification deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle specification", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle inspections
    /// </summary>
    [HttpGet("{id}/inspections")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleInspectionDto>>>> GetVehicleInspections(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleInspections
                .Where(i => i.VehicleId == id)
                .Select(i => new VehicleInspectionDto
                {
                    Id = i.Id,
                    VehicleId = i.VehicleId,
                    InspectionDate = i.InspectionDate,
                    InspectionType = i.InspectionType,
                    InspectorName = i.InspectorName,
                    InspectionCenter = i.InspectionCenter,
                    Status = i.Status,
                    CertificateNumber = i.CertificateNumber,
                    CertificateExpiryDate = i.CertificateExpiryDate,
                    Mileage = i.Mileage,
                    DefectsFound = i.DefectsFound,
                    RepairsRequired = i.RepairsRequired,
                    RepairsCompleted = i.RepairsCompleted,
                    InspectionFee = i.InspectionFee,
                    IsPassed = i.Status == "Passed",
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt
                })
                .OrderByDescending(i => i.InspectionDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleInspectionDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleInspectionDto>>.ErrorResponse("Error retrieving vehicle inspections", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle inspection
    /// </summary>
    [HttpPut("{id}/inspections/{inspectionId}")]
    public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> UpdateVehicleInspection(Guid id, Guid inspectionId, UpdateVehicleInspectionDto dto)
    {
        try
        {
            var inspection = await _context.VehicleInspections
                .FirstOrDefaultAsync(i => i.Id == inspectionId && i.VehicleId == id);

            if (inspection == null)
            {
                return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResponse("Vehicle inspection not found"));
            }

            inspection.InspectionDate = dto.InspectionDate;
            inspection.InspectionType = dto.InspectionType;
            inspection.InspectorName = dto.InspectorName;
            inspection.InspectionCenter = dto.InspectionCenter;
            inspection.Status = dto.Status;
            inspection.Mileage = dto.Mileage;
            inspection.CertificateNumber = dto.CertificateNumber;
            inspection.CertificateExpiryDate = dto.CertificateExpiryDate;
            inspection.DefectsFound = dto.DefectsFound;
            inspection.RepairsRequired = dto.RepairsRequired;
            inspection.RepairsCompleted = dto.RepairsCompleted;
            inspection.InspectionFee = dto.InspectionFee;
            inspection.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleInspectionDto
            {
                Id = inspection.Id,
                VehicleId = inspection.VehicleId,
                InspectionDate = inspection.InspectionDate,
                InspectionType = inspection.InspectionType,
                InspectorName = inspection.InspectorName,
                InspectionCenter = inspection.InspectionCenter,
                Status = inspection.Status,
                CertificateNumber = inspection.CertificateNumber,
                CertificateExpiryDate = inspection.CertificateExpiryDate,
                Mileage = inspection.Mileage,
                DefectsFound = inspection.DefectsFound,
                RepairsRequired = inspection.RepairsRequired,
                RepairsCompleted = inspection.RepairsCompleted,
                InspectionFee = inspection.InspectionFee,
                IsPassed = inspection.Status == "Passed",
                Notes = inspection.Notes,
                CreatedAt = inspection.CreatedAt
            };

            return Ok(ApiResponse<VehicleInspectionDto>.SuccessResponse(responseDto, "Vehicle inspection updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResponse("Error updating vehicle inspection", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle inspection
    /// </summary>
    [HttpPost("{id}/inspections")]
    public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> CreateVehicleInspection(Guid id, CreateVehicleInspectionDto dto)
    {
        try
        {
            var inspection = new VehicleInspection
            {
                Id = Guid.NewGuid(),
                VehicleId = id,
                InspectionDate = dto.InspectionDate,
                InspectionType = dto.InspectionType,
                InspectorName = dto.InspectorName,
                InspectionCenter = dto.InspectionCenter,
                Status = dto.Status,
                Mileage = dto.Mileage
            };

            _context.VehicleInspections.Add(inspection);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleInspectionDto
            {
                Id = inspection.Id,
                VehicleId = inspection.VehicleId,
                InspectionDate = inspection.InspectionDate,
                InspectionType = inspection.InspectionType,
                InspectorName = inspection.InspectorName,
                InspectionCenter = inspection.InspectionCenter,
                Status = inspection.Status,
                Mileage = inspection.Mileage,
                IsPassed = inspection.Status == "Passed",
                CreatedAt = inspection.CreatedAt
            };

            return Ok(ApiResponse<VehicleInspectionDto>.SuccessResponse(responseDto, "Vehicle inspection created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResponse("Error creating vehicle inspection", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle inspection
    /// </summary>
    [HttpPatch("{id}/inspections/{inspectionId}")]
    public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> PatchVehicleInspection(Guid id, Guid inspectionId, PatchVehicleInspectionDto dto)
    {
        try
        {
            var inspection = await _context.VehicleInspections
                .FirstOrDefaultAsync(i => i.Id == inspectionId && i.VehicleId == id);

            if (inspection == null)
            {
                return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResponse("Vehicle inspection not found"));
            }

            if (dto.InspectionDate.HasValue) inspection.InspectionDate = dto.InspectionDate.Value;
            if (dto.InspectionType != null) inspection.InspectionType = dto.InspectionType;
            if (dto.InspectorName != null) inspection.InspectorName = dto.InspectorName;
            if (dto.InspectionCenter != null) inspection.InspectionCenter = dto.InspectionCenter;
            if (dto.Status != null) inspection.Status = dto.Status;
            if (dto.Mileage.HasValue) inspection.Mileage = dto.Mileage;
            if (dto.CertificateNumber != null) inspection.CertificateNumber = dto.CertificateNumber;
            if (dto.CertificateExpiryDate.HasValue) inspection.CertificateExpiryDate = dto.CertificateExpiryDate;
            if (dto.DefectsFound != null) inspection.DefectsFound = dto.DefectsFound;
            if (dto.RepairsRequired != null) inspection.RepairsRequired = dto.RepairsRequired;
            if (dto.RepairsCompleted != null) inspection.RepairsCompleted = dto.RepairsCompleted;
            if (dto.InspectionFee.HasValue) inspection.InspectionFee = dto.InspectionFee;
            if (dto.Notes != null) inspection.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleInspectionDto
            {
                Id = inspection.Id,
                VehicleId = inspection.VehicleId,
                InspectionDate = inspection.InspectionDate,
                InspectionType = inspection.InspectionType,
                InspectorName = inspection.InspectorName,
                InspectionCenter = inspection.InspectionCenter,
                Status = inspection.Status,
                CertificateNumber = inspection.CertificateNumber,
                CertificateExpiryDate = inspection.CertificateExpiryDate,
                Mileage = inspection.Mileage,
                DefectsFound = inspection.DefectsFound,
                RepairsRequired = inspection.RepairsRequired,
                RepairsCompleted = inspection.RepairsCompleted,
                InspectionFee = inspection.InspectionFee,
                IsPassed = inspection.Status == "Passed",
                Notes = inspection.Notes,
                CreatedAt = inspection.CreatedAt
            };

            return Ok(ApiResponse<VehicleInspectionDto>.SuccessResponse(responseDto, "Vehicle inspection updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResponse("Error updating vehicle inspection", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle inspection
    /// </summary>
    [HttpDelete("{id}/inspections/{inspectionId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleInspection(Guid id, Guid inspectionId)
    {
        try
        {
            var inspection = await _context.VehicleInspections
                .FirstOrDefaultAsync(i => i.Id == inspectionId && i.VehicleId == id);

            if (inspection == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle inspection not found"));
            }

            _context.VehicleInspections.Remove(inspection);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle inspection deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle inspection", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle insurance policies
    /// </summary>
    [HttpGet("{id}/insurance")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleInsuranceDto>>>> GetVehicleInsurance(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleInsurances
                .Where(i => i.VehicleId == id)
                .Select(i => new VehicleInsuranceDto
                {
                    Id = i.Id,
                    VehicleId = i.VehicleId,
                    PolicyNumber = i.PolicyNumber,
                    InsuranceProvider = i.InsuranceProvider,
                    PolicyType = i.PolicyType,
                    StartDate = i.StartDate,
                    EndDate = i.EndDate,
                    PremiumAmount = i.PremiumAmount,
                    CoverageAmount = i.CoverageAmount,
                    Deductible = i.Deductible,
                    Status = i.Status,
                    ContactPerson = i.ContactPerson,
                    ContactPhone = i.ContactPhone,
                    CoverageDetails = i.CoverageDetails,
                    Claims = i.Claims,
                    LastClaimDate = i.LastClaimDate,
                    IsExpired = i.EndDate < DateTime.Today,
                    DaysUntilExpiry = (int)(i.EndDate - DateTime.Today).TotalDays,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt
                })
                .OrderByDescending(i => i.StartDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleInsuranceDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleInsuranceDto>>.ErrorResponse("Error retrieving vehicle insurance", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle insurance policy
    /// </summary>
    [HttpPut("{id}/insurance/{insuranceId}")]
    public async Task<ActionResult<ApiResponse<VehicleInsuranceDto>>> UpdateVehicleInsurance(Guid id, Guid insuranceId, UpdateVehicleInsuranceDto dto)
    {
        try
        {
            var insurance = await _context.VehicleInsurances
                .FirstOrDefaultAsync(i => i.Id == insuranceId && i.VehicleId == id);

            if (insurance == null)
            {
                return NotFound(ApiResponse<VehicleInsuranceDto>.ErrorResponse("Vehicle insurance not found"));
            }

            insurance.PolicyNumber = dto.PolicyNumber;
            insurance.InsuranceProvider = dto.InsuranceProvider;
            insurance.PolicyType = dto.PolicyType;
            insurance.StartDate = dto.StartDate;
            insurance.EndDate = dto.EndDate;
            insurance.PremiumAmount = dto.PremiumAmount;
            insurance.CoverageAmount = dto.CoverageAmount;
            insurance.Deductible = dto.Deductible;
            insurance.Status = dto.Status;
            insurance.ContactPerson = dto.ContactPerson;
            insurance.ContactPhone = dto.ContactPhone;
            insurance.CoverageDetails = dto.CoverageDetails;
            insurance.Claims = dto.Claims;
            insurance.LastClaimDate = dto.LastClaimDate;
            insurance.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleInsuranceDto
            {
                Id = insurance.Id,
                VehicleId = insurance.VehicleId,
                PolicyNumber = insurance.PolicyNumber,
                InsuranceProvider = insurance.InsuranceProvider,
                PolicyType = insurance.PolicyType,
                StartDate = insurance.StartDate,
                EndDate = insurance.EndDate,
                PremiumAmount = insurance.PremiumAmount,
                CoverageAmount = insurance.CoverageAmount,
                Deductible = insurance.Deductible,
                Status = insurance.Status,
                ContactPerson = insurance.ContactPerson,
                ContactPhone = insurance.ContactPhone,
                CoverageDetails = insurance.CoverageDetails,
                Claims = insurance.Claims,
                LastClaimDate = insurance.LastClaimDate,
                IsExpired = insurance.EndDate < DateTime.Today,
                DaysUntilExpiry = (int)(insurance.EndDate - DateTime.Today).TotalDays,
                Notes = insurance.Notes,
                CreatedAt = insurance.CreatedAt
            };

            return Ok(ApiResponse<VehicleInsuranceDto>.SuccessResponse(responseDto, "Vehicle insurance updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleInsuranceDto>.ErrorResponse("Error updating vehicle insurance", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle insurance policy
    /// </summary>
    [HttpPost("{id}/insurance")]
    public async Task<ActionResult<ApiResponse<VehicleInsuranceDto>>> CreateVehicleInsurance(Guid id, CreateVehicleInsuranceDto dto)
    {
        try
        {
            var insurance = new VehicleInsurance
            {
                Id = Guid.NewGuid(),
                VehicleId = id,
                PolicyNumber = dto.PolicyNumber,
                InsuranceProvider = dto.InsuranceProvider,
                PolicyType = dto.PolicyType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PremiumAmount = dto.PremiumAmount,
                CoverageAmount = dto.CoverageAmount,
                Deductible = dto.Deductible,
                Status = "Active"
            };

            _context.VehicleInsurances.Add(insurance);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleInsuranceDto
            {
                Id = insurance.Id,
                VehicleId = insurance.VehicleId,
                PolicyNumber = insurance.PolicyNumber,
                InsuranceProvider = insurance.InsuranceProvider,
                PolicyType = insurance.PolicyType,
                StartDate = insurance.StartDate,
                EndDate = insurance.EndDate,
                PremiumAmount = insurance.PremiumAmount,
                CoverageAmount = insurance.CoverageAmount,
                Deductible = insurance.Deductible,
                Status = insurance.Status,
                IsExpired = insurance.EndDate < DateTime.Today,
                DaysUntilExpiry = (int)(insurance.EndDate - DateTime.Today).TotalDays,
                CreatedAt = insurance.CreatedAt
            };

            return Ok(ApiResponse<VehicleInsuranceDto>.SuccessResponse(responseDto, "Vehicle insurance created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleInsuranceDto>.ErrorResponse("Error creating vehicle insurance", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle insurance policy
    /// </summary>
    [HttpPatch("{id}/insurance/{insuranceId}")]
    public async Task<ActionResult<ApiResponse<VehicleInsuranceDto>>> PatchVehicleInsurance(Guid id, Guid insuranceId, PatchVehicleInsuranceDto dto)
    {
        try
        {
            var insurance = await _context.VehicleInsurances
                .FirstOrDefaultAsync(i => i.Id == insuranceId && i.VehicleId == id);

            if (insurance == null)
            {
                return NotFound(ApiResponse<VehicleInsuranceDto>.ErrorResponse("Vehicle insurance not found"));
            }

            if (dto.PolicyNumber != null) insurance.PolicyNumber = dto.PolicyNumber;
            if (dto.InsuranceProvider != null) insurance.InsuranceProvider = dto.InsuranceProvider;
            if (dto.PolicyType != null) insurance.PolicyType = dto.PolicyType;
            if (dto.StartDate.HasValue) insurance.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) insurance.EndDate = dto.EndDate.Value;
            if (dto.PremiumAmount.HasValue) insurance.PremiumAmount = dto.PremiumAmount.Value;
            if (dto.CoverageAmount.HasValue) insurance.CoverageAmount = dto.CoverageAmount.Value;
            if (dto.Deductible.HasValue) insurance.Deductible = dto.Deductible.Value;
            if (dto.Status != null) insurance.Status = dto.Status;
            if (dto.ContactPerson != null) insurance.ContactPerson = dto.ContactPerson;
            if (dto.ContactPhone != null) insurance.ContactPhone = dto.ContactPhone;
            if (dto.CoverageDetails != null) insurance.CoverageDetails = dto.CoverageDetails;
            if (dto.Claims != null) insurance.Claims = dto.Claims;
            if (dto.LastClaimDate.HasValue) insurance.LastClaimDate = dto.LastClaimDate;
            if (dto.Notes != null) insurance.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleInsuranceDto
            {
                Id = insurance.Id,
                VehicleId = insurance.VehicleId,
                PolicyNumber = insurance.PolicyNumber,
                InsuranceProvider = insurance.InsuranceProvider,
                PolicyType = insurance.PolicyType,
                StartDate = insurance.StartDate,
                EndDate = insurance.EndDate,
                PremiumAmount = insurance.PremiumAmount,
                CoverageAmount = insurance.CoverageAmount,
                Deductible = insurance.Deductible,
                Status = insurance.Status,
                ContactPerson = insurance.ContactPerson,
                ContactPhone = insurance.ContactPhone,
                CoverageDetails = insurance.CoverageDetails,
                Claims = insurance.Claims,
                LastClaimDate = insurance.LastClaimDate,
                IsExpired = insurance.EndDate < DateTime.Today,
                DaysUntilExpiry = (int)(insurance.EndDate - DateTime.Today).TotalDays,
                Notes = insurance.Notes,
                CreatedAt = insurance.CreatedAt
            };

            return Ok(ApiResponse<VehicleInsuranceDto>.SuccessResponse(responseDto, "Vehicle insurance updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleInsuranceDto>.ErrorResponse("Error updating vehicle insurance", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle insurance policy
    /// </summary>
    [HttpDelete("{id}/insurance/{insuranceId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleInsurance(Guid id, Guid insuranceId)
    {
        try
        {
            var insurance = await _context.VehicleInsurances
                .FirstOrDefaultAsync(i => i.Id == insuranceId && i.VehicleId == id);

            if (insurance == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle insurance not found"));
            }

            _context.VehicleInsurances.Remove(insurance);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle insurance deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle insurance", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle maintenance records
    /// </summary>
    [HttpGet("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleMaintenanceDto>>>> GetVehicleMaintenance(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleMaintenances
                .Where(m => m.VehicleId == id)
                .Select(m => new VehicleMaintenanceDto
                {
                    Id = m.Id,
                    VehicleId = m.VehicleId,
                    MaintenanceDate = m.MaintenanceDate,
                    MaintenanceType = m.MaintenanceType,
                    ServiceProvider = m.ServiceProvider,
                    Mileage = m.Mileage,
                    ServicesPerformed = m.ServicesPerformed,
                    PartsReplaced = m.PartsReplaced,
                    ServiceCost = m.ServiceCost,
                    PartsCost = m.PartsCost,
                    TotalCost = m.TotalCost,
                    Status = m.Status,
                    NextServiceDate = m.NextServiceDate,
                    NextServiceMileage = m.NextServiceMileage,
                    Warranty = m.Warranty,
                    PerformedBy = m.PerformedBy,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt
                })
                .OrderByDescending(m => m.MaintenanceDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleMaintenanceDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleMaintenanceDto>>.ErrorResponse("Error retrieving vehicle maintenance records", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle maintenance record
    /// </summary>
    [HttpPut("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<VehicleMaintenanceDto>>> UpdateVehicleMaintenance(Guid id, Guid maintenanceId, UpdateVehicleMaintenanceDto dto)
    {
        try
        {
            var maintenance = await _context.VehicleMaintenances
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.VehicleId == id);

            if (maintenance == null)
            {
                return NotFound(ApiResponse<VehicleMaintenanceDto>.ErrorResponse("Vehicle maintenance record not found"));
            }

            maintenance.MaintenanceDate = dto.MaintenanceDate;
            maintenance.MaintenanceType = dto.MaintenanceType;
            maintenance.ServiceProvider = dto.ServiceProvider;
            maintenance.Mileage = dto.Mileage;
            maintenance.ServiceCost = dto.ServiceCost;
            maintenance.PartsCost = dto.PartsCost;
            maintenance.TotalCost = dto.ServiceCost + dto.PartsCost;
            maintenance.PerformedBy = dto.PerformedBy;
            maintenance.Status = dto.Status;
            maintenance.ServicesPerformed = dto.ServicesPerformed;
            maintenance.PartsReplaced = dto.PartsReplaced;
            maintenance.NextServiceDate = dto.NextServiceDate;
            maintenance.NextServiceMileage = dto.NextServiceMileage;
            maintenance.Warranty = dto.Warranty;
            maintenance.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleMaintenanceDto
            {
                Id = maintenance.Id,
                VehicleId = maintenance.VehicleId,
                MaintenanceDate = maintenance.MaintenanceDate,
                MaintenanceType = maintenance.MaintenanceType,
                ServiceProvider = maintenance.ServiceProvider,
                Mileage = maintenance.Mileage,
                ServicesPerformed = maintenance.ServicesPerformed,
                PartsReplaced = maintenance.PartsReplaced,
                ServiceCost = maintenance.ServiceCost,
                PartsCost = maintenance.PartsCost,
                TotalCost = maintenance.TotalCost,
                Status = maintenance.Status,
                NextServiceDate = maintenance.NextServiceDate,
                NextServiceMileage = maintenance.NextServiceMileage,
                Warranty = maintenance.Warranty,
                PerformedBy = maintenance.PerformedBy,
                Notes = maintenance.Notes,
                CreatedAt = maintenance.CreatedAt
            };

            return Ok(ApiResponse<VehicleMaintenanceDto>.SuccessResponse(responseDto, "Vehicle maintenance record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleMaintenanceDto>.ErrorResponse("Error updating vehicle maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle maintenance record
    /// </summary>
    [HttpPost("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<VehicleMaintenanceDto>>> CreateVehicleMaintenance(Guid id, CreateVehicleMaintenanceDto dto)
    {
        try
        {
            var maintenance = new VehicleMaintenance
            {
                Id = Guid.NewGuid(),
                VehicleId = id,
                MaintenanceDate = dto.MaintenanceDate,
                MaintenanceType = dto.MaintenanceType,
                ServiceProvider = dto.ServiceProvider,
                Mileage = dto.Mileage,
                ServiceCost = dto.ServiceCost,
                PartsCost = dto.PartsCost,
                TotalCost = dto.ServiceCost + dto.PartsCost,
                PerformedBy = dto.PerformedBy,
                Status = "Completed"
            };

            _context.VehicleMaintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleMaintenanceDto
            {
                Id = maintenance.Id,
                VehicleId = maintenance.VehicleId,
                MaintenanceDate = maintenance.MaintenanceDate,
                MaintenanceType = maintenance.MaintenanceType,
                ServiceProvider = maintenance.ServiceProvider,
                Mileage = maintenance.Mileage,
                ServiceCost = maintenance.ServiceCost,
                PartsCost = maintenance.PartsCost,
                TotalCost = maintenance.TotalCost,
                Status = maintenance.Status,
                PerformedBy = maintenance.PerformedBy,
                CreatedAt = maintenance.CreatedAt
            };

            return Ok(ApiResponse<VehicleMaintenanceDto>.SuccessResponse(responseDto, "Vehicle maintenance record created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleMaintenanceDto>.ErrorResponse("Error creating vehicle maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle maintenance record
    /// </summary>
    [HttpPatch("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<VehicleMaintenanceDto>>> PatchVehicleMaintenance(Guid id, Guid maintenanceId, PatchVehicleMaintenanceDto dto)
    {
        try
        {
            var maintenance = await _context.VehicleMaintenances
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.VehicleId == id);

            if (maintenance == null)
            {
                return NotFound(ApiResponse<VehicleMaintenanceDto>.ErrorResponse("Vehicle maintenance record not found"));
            }

            if (dto.MaintenanceDate.HasValue) maintenance.MaintenanceDate = dto.MaintenanceDate.Value;
            if (dto.MaintenanceType != null) maintenance.MaintenanceType = dto.MaintenanceType;
            if (dto.ServiceProvider != null) maintenance.ServiceProvider = dto.ServiceProvider;
            if (dto.Mileage.HasValue) maintenance.Mileage = dto.Mileage;
            if (dto.ServiceCost.HasValue) maintenance.ServiceCost = dto.ServiceCost.Value;
            if (dto.PartsCost.HasValue) maintenance.PartsCost = dto.PartsCost.Value;
            if (dto.ServiceCost.HasValue || dto.PartsCost.HasValue) maintenance.TotalCost = maintenance.ServiceCost + maintenance.PartsCost;
            if (dto.PerformedBy != null) maintenance.PerformedBy = dto.PerformedBy;
            if (dto.Status != null) maintenance.Status = dto.Status;
            if (dto.ServicesPerformed != null) maintenance.ServicesPerformed = dto.ServicesPerformed;
            if (dto.PartsReplaced != null) maintenance.PartsReplaced = dto.PartsReplaced;
            if (dto.NextServiceDate.HasValue) maintenance.NextServiceDate = dto.NextServiceDate;
            if (dto.NextServiceMileage.HasValue) maintenance.NextServiceMileage = dto.NextServiceMileage;
            if (dto.Warranty != null) maintenance.Warranty = dto.Warranty;
            if (dto.Notes != null) maintenance.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleMaintenanceDto
            {
                Id = maintenance.Id,
                VehicleId = maintenance.VehicleId,
                MaintenanceDate = maintenance.MaintenanceDate,
                MaintenanceType = maintenance.MaintenanceType,
                ServiceProvider = maintenance.ServiceProvider,
                Mileage = maintenance.Mileage,
                ServicesPerformed = maintenance.ServicesPerformed,
                PartsReplaced = maintenance.PartsReplaced,
                ServiceCost = maintenance.ServiceCost,
                PartsCost = maintenance.PartsCost,
                TotalCost = maintenance.TotalCost,
                Status = maintenance.Status,
                NextServiceDate = maintenance.NextServiceDate,
                NextServiceMileage = maintenance.NextServiceMileage,
                Warranty = maintenance.Warranty,
                PerformedBy = maintenance.PerformedBy,
                Notes = maintenance.Notes,
                CreatedAt = maintenance.CreatedAt
            };

            return Ok(ApiResponse<VehicleMaintenanceDto>.SuccessResponse(responseDto, "Vehicle maintenance record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleMaintenanceDto>.ErrorResponse("Error updating vehicle maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle maintenance record
    /// </summary>
    [HttpDelete("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleMaintenance(Guid id, Guid maintenanceId)
    {
        try
        {
            var maintenance = await _context.VehicleMaintenances
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.VehicleId == id);

            if (maintenance == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle maintenance record not found"));
            }

            _context.VehicleMaintenances.Remove(maintenance);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle maintenance record deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle documents
    /// </summary>
    [HttpGet("{id}/documents")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleDocumentDto>>>> GetVehicleDocuments(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleDocuments
                .Where(d => d.VehicleId == id)
                .Select(d => new VehicleDocumentDto
                {
                    Id = d.Id,
                    VehicleId = d.VehicleId,
                    FileName = d.FileName,
                    OriginalFileName = d.OriginalFileName,
                    ContentType = d.ContentType,
                    FilePath = d.FilePath,
                    FileUrl = d.FileUrl,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Description = d.Description,
                    ExpiryDate = d.ExpiryDate,
                    IsActive = d.IsActive,
                    UploadedBy = d.UploadedBy,
                    UploadedAt = d.UploadedAt,
                    IsExpired = d.ExpiryDate.HasValue && d.ExpiryDate < DateTime.Today
                })
                .OrderByDescending(d => d.UploadedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleDocumentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleDocumentDto>>.ErrorResponse("Error retrieving vehicle documents", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle document
    /// </summary>
    [HttpPost("{id}/documents")]
    public async Task<ActionResult<ApiResponse<VehicleDocumentDto>>> CreateVehicleDocument(Guid id, CreateVehicleDocumentDto dto)
    {
        try
        {
            var document = new VehicleDocument
            {
                Id = Guid.NewGuid(),
                VehicleId = id,
                FileName = dto.FileName,
                OriginalFileName = dto.OriginalFileName,
                ContentType = dto.ContentType,
                FilePath = dto.FilePath,
                FileUrl = dto.FileUrl,
                FileSize = dto.FileSize,
                Category = dto.Category,
                Description = dto.Description,
                ExpiryDate = dto.ExpiryDate,
                IsActive = true,
                UploadedBy = dto.UploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            _context.VehicleDocuments.Add(document);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleDocumentDto
            {
                Id = document.Id,
                VehicleId = document.VehicleId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<VehicleDocumentDto>.SuccessResponse(responseDto, "Vehicle document created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDocumentDto>.ErrorResponse("Error creating vehicle document", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle document
    /// </summary>
    [HttpPut("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<VehicleDocumentDto>>> UpdateVehicleDocument(Guid id, Guid documentId, UpdateVehicleDocumentDto dto)
    {
        try
        {
            var document = await _context.VehicleDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.VehicleId == id);

            if (document == null)
            {
                return NotFound(ApiResponse<VehicleDocumentDto>.ErrorResponse("Vehicle document not found"));
            }

            document.FileName = dto.FileName;
            document.OriginalFileName = dto.OriginalFileName;
            document.ContentType = dto.ContentType;
            document.FilePath = dto.FilePath;
            document.FileUrl = dto.FileUrl;
            document.FileSize = dto.FileSize;
            document.Category = dto.Category;
            document.Description = dto.Description;
            document.ExpiryDate = dto.ExpiryDate;
            document.IsActive = dto.IsActive;
            document.UploadedBy = dto.UploadedBy;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleDocumentDto
            {
                Id = document.Id,
                VehicleId = document.VehicleId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<VehicleDocumentDto>.SuccessResponse(responseDto, "Vehicle document updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDocumentDto>.ErrorResponse("Error updating vehicle document", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle document
    /// </summary>
    [HttpPatch("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<VehicleDocumentDto>>> PatchVehicleDocument(Guid id, Guid documentId, PatchVehicleDocumentDto dto)
    {
        try
        {
            var document = await _context.VehicleDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.VehicleId == id);

            if (document == null)
            {
                return NotFound(ApiResponse<VehicleDocumentDto>.ErrorResponse("Vehicle document not found"));
            }

            if (dto.FileName != null) document.FileName = dto.FileName;
            if (dto.OriginalFileName != null) document.OriginalFileName = dto.OriginalFileName;
            if (dto.ContentType != null) document.ContentType = dto.ContentType;
            if (dto.FilePath != null) document.FilePath = dto.FilePath;
            if (dto.FileUrl != null) document.FileUrl = dto.FileUrl;
            if (dto.FileSize.HasValue) document.FileSize = dto.FileSize.Value;
            if (dto.Category != null) document.Category = dto.Category;
            if (dto.Description != null) document.Description = dto.Description;
            if (dto.ExpiryDate.HasValue) document.ExpiryDate = dto.ExpiryDate;
            if (dto.IsActive.HasValue) document.IsActive = dto.IsActive.Value;
            if (dto.UploadedBy != null) document.UploadedBy = dto.UploadedBy;

            await _context.SaveChangesAsync();

            var responseDto = new VehicleDocumentDto
            {
                Id = document.Id,
                VehicleId = document.VehicleId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<VehicleDocumentDto>.SuccessResponse(responseDto, "Vehicle document updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleDocumentDto>.ErrorResponse("Error updating vehicle document", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle document
    /// </summary>
    [HttpDelete("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleDocument(Guid id, Guid documentId)
    {
        try
        {
            var document = await _context.VehicleDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.VehicleId == id);

            if (document == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle document not found"));
            }

            _context.VehicleDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle document deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle document", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle ownership information (which transporter owns this vehicle)
    /// </summary>
    [HttpGet("{id}/ownership")]
    public async Task<ActionResult<ApiResponse<VehicleTransporterOwnershipDto>>> GetVehicleOwnership(Guid id)
    {
        try
        {
            if (!await VehicleExists(id))
            {
                return NotFound(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle not found"));
            }

            var ownership = await _context.VehicleTransporterOwnerships
                .Where(o => o.VehicleId == id && o.IsActive)
                .Select(o => new VehicleTransporterOwnershipDto
                {
                    Id = o.Id,
                    VehicleId = o.VehicleId,
                    TransporterProfileId = o.TransporterProfileId,
                    OwnershipStartDate = o.OwnershipStartDate,
                    OwnershipEndDate = o.OwnershipEndDate,
                    OwnershipType = o.OwnershipType,
                    PurchasePrice = o.PurchasePrice,
                    CurrentValue = o.CurrentValue,
                    FinancingDetails = o.FinancingDetails,
                    InsuranceDetails = o.InsuranceDetails,
                    IsActive = o.IsActive,
                    Notes = o.Notes,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity
                    TransporterName = "Transporter Name" // TODO: Join with Transporter entity
                })
                .FirstOrDefaultAsync();

            if (ownership == null)
            {
                return NotFound(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle ownership not found"));
            }

            return Ok(ApiResponse<VehicleTransporterOwnershipDto>.SuccessResponse(ownership));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Error retrieving vehicle ownership", ex.Message));
        }
    }

    /// <summary>
    /// Set or update vehicle ownership (assign vehicle to transporter)
    /// </summary>
    [HttpPut("{id}/ownership")]
    public async Task<ActionResult<ApiResponse<VehicleTransporterOwnershipDto>>> SetVehicleOwnership(
        Guid id, CreateVehicleTransporterOwnershipDto dto)
    {
        try
        {
            if (!await VehicleExists(id))
            {
                return NotFound(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle not found"));
            }

            if (dto.VehicleId != id)
            {
                return BadRequest(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle ID mismatch"));
            }

            // Deactivate existing ownership
            var existingOwnership = await _context.VehicleTransporterOwnerships
                .FirstOrDefaultAsync(o => o.VehicleId == id && o.IsActive);
            
            if (existingOwnership != null)
            {
                existingOwnership.IsActive = false;
                existingOwnership.OwnershipEndDate = DateTime.UtcNow;
                existingOwnership.UpdatedAt = DateTime.UtcNow;
            }

            // Create new ownership record
            var ownership = new VehicleTransporterOwnership
            {
                Id = Guid.NewGuid(),
                VehicleId = dto.VehicleId,
                TransporterProfileId = dto.TransporterProfileId,
                OwnershipStartDate = dto.OwnershipStartDate,
                OwnershipEndDate = dto.OwnershipEndDate,
                OwnershipType = dto.OwnershipType,
                PurchasePrice = dto.PurchasePrice,
                CurrentValue = dto.CurrentValue,
                FinancingDetails = dto.FinancingDetails,
                InsuranceDetails = dto.InsuranceDetails,
                Notes = dto.Notes
            };

            _context.VehicleTransporterOwnerships.Add(ownership);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleTransporterOwnershipDto
            {
                Id = ownership.Id,
                VehicleId = ownership.VehicleId,
                TransporterProfileId = ownership.TransporterProfileId,
                OwnershipStartDate = ownership.OwnershipStartDate,
                OwnershipEndDate = ownership.OwnershipEndDate,
                OwnershipType = ownership.OwnershipType,
                PurchasePrice = ownership.PurchasePrice,
                CurrentValue = ownership.CurrentValue,
                FinancingDetails = ownership.FinancingDetails,
                InsuranceDetails = ownership.InsuranceDetails,
                IsActive = ownership.IsActive,
                Notes = ownership.Notes,
                CreatedAt = ownership.CreatedAt,
                UpdatedAt = ownership.UpdatedAt,
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity
                TransporterName = "Transporter Name" // TODO: Join with Transporter entity
            };

            return Ok(ApiResponse<VehicleTransporterOwnershipDto>.SuccessResponse(responseDto, "Vehicle ownership updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Error setting vehicle ownership", ex.Message));
        }
    }

    /// <summary>
    /// Get current driver assignments for this vehicle
    /// </summary>
    [HttpGet("{id}/drivers")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverVehicleAssignmentDto>>>> GetVehicleDrivers(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            if (!await VehicleExists(id))
            {
                return NotFound(ApiResponse<PagedResult<DriverVehicleAssignmentDto>>.ErrorResponse("Vehicle not found"));
            }

            var query = _context.DriverVehicleAssignments
                .Where(a => a.VehicleId == id)
                .Select(a => new DriverVehicleAssignmentDto
                {
                    Id = a.Id,
                    DriverId = a.DriverId,
                    VehicleId = a.VehicleId,
                    AssignedDate = a.AssignedDate,
                    UnassignedDate = a.UnassignedDate,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    AssignmentType = a.AssignmentType,
                    Reason = a.Reason,
                    AssignedBy = a.AssignedBy,
                    UnassignedBy = a.UnassignedBy,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    DriverName = "Driver Name", // TODO: Join with Driver entity
                    VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity
                    IsCurrentlyAssigned = a.IsActive && a.UnassignedDate == null
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverVehicleAssignmentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverVehicleAssignmentDto>>.ErrorResponse("Error retrieving vehicle drivers", ex.Message));
        }
    }

    /// <summary>
    /// Assign a driver to this vehicle
    /// </summary>
    [HttpPost("{id}/drivers")]
    public async Task<ActionResult<ApiResponse<DriverVehicleAssignmentDto>>> AssignDriverToVehicle(
        Guid id, CreateDriverVehicleAssignmentDto dto)
    {
        try
        {
            if (!await VehicleExists(id))
            {
                return NotFound(ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Vehicle not found"));
            }

            if (dto.VehicleId != id)
            {
                return BadRequest(ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Vehicle ID mismatch"));
            }

            var assignment = new DriverVehicleAssignment
            {
                Id = Guid.NewGuid(),
                DriverId = dto.DriverId,
                VehicleId = dto.VehicleId,
                AssignedDate = dto.AssignedDate,
                UnassignedDate = dto.UnassignedDate,
                IsPrimary = dto.IsPrimary,
                AssignmentType = dto.AssignmentType,
                Reason = dto.Reason,
                AssignedBy = dto.AssignedBy,
                Notes = dto.Notes
            };

            _context.DriverVehicleAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var responseDto = new DriverVehicleAssignmentDto
            {
                Id = assignment.Id,
                DriverId = assignment.DriverId,
                VehicleId = assignment.VehicleId,
                AssignedDate = assignment.AssignedDate,
                UnassignedDate = assignment.UnassignedDate,
                IsPrimary = assignment.IsPrimary,
                IsActive = assignment.IsActive,
                AssignmentType = assignment.AssignmentType,
                Reason = assignment.Reason,
                AssignedBy = assignment.AssignedBy,
                UnassignedBy = assignment.UnassignedBy,
                Notes = assignment.Notes,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt,
                DriverName = "Driver Name", // TODO: Join with Driver entity
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity
                IsCurrentlyAssigned = assignment.IsActive && assignment.UnassignedDate == null
            };

            return Ok(ApiResponse<DriverVehicleAssignmentDto>.SuccessResponse(responseDto, "Driver assigned to vehicle successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Error assigning driver to vehicle", ex.Message));
        }
    }

    private async Task<bool> VehicleExists(Guid id)
    {
        return await _context.Vehicles.AnyAsync(e => e.Id == id);
    }
}