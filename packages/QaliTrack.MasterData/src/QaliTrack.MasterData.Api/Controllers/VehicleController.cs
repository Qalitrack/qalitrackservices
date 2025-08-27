using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Core.Modules.Vehicle.DTOs;
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

    private async Task<bool> VehicleExists(Guid id)
    {
        return await _context.Vehicles.AnyAsync(e => e.Id == id);
    }
}