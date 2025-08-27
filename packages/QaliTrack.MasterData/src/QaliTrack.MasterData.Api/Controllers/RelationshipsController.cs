using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Relationships.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.Entities;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Core.Modules.Sacco.Entities;
using QaliTrack.MasterData.Core.Modules.Product.Entities;
using QaliTrack.MasterData.Core.Modules.Route.Entities;
using QaliTrack.MasterData.Core.Modules.Weighbridge.Entities;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Cross-Module Relationships")]
public class RelationshipsController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public RelationshipsController(MasterDataDbContext context)
    {
        _context = context;
    }

    #region Driver-SACCO Membership

    /// <summary>
    /// Get driver-sacco memberships with pagination
    /// </summary>
    [HttpGet("driver-sacco-memberships")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverSaccoMembershipDto>>>> GetDriverSaccoMemberships(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverSaccoMemberships
                .Include(m => m.Driver)
                .Include(m => m.Sacco)
                .Select(m => new DriverSaccoMembershipDto
                {
                    Id = m.Id,
                    DriverId = m.DriverId,
                    SaccoId = m.SaccoId,
                    MembershipDate = m.MembershipDate,
                    ExpiryDate = m.ExpiryDate,
                    MembershipNumber = m.MembershipNumber,
                    Status = m.Status,
                    ShareContribution = m.ShareContribution,
                    MonthlyContribution = m.MonthlyContribution,
                    MembershipType = m.MembershipType,
                    Benefits = m.Benefits,
                    IsActive = m.IsActive,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    IsExpired = m.ExpiryDate.HasValue && m.ExpiryDate < DateTime.Today,
                    DriverName = m.Driver != null ? $"{m.Driver.FirstName} {m.Driver.LastName}".Trim() : null,
                    SaccoName = m.Sacco != null ? m.Sacco.Name : null
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverSaccoMembershipDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverSaccoMembershipDto>>.ErrorResponse("Error retrieving driver-sacco memberships", ex.Message));
        }
    }

    /// <summary>
    /// Get specific driver-sacco membership
    /// </summary>
    [HttpGet("driver-sacco-memberships/{id}")]
    public async Task<ActionResult<ApiResponse<DriverSaccoMembershipDto>>> GetDriverSaccoMembership(Guid id)
    {
        try
        {
            var membership = await _context.DriverSaccoMemberships
                .Include(m => m.Driver)
                .Include(m => m.Sacco)
                .Where(m => m.Id == id)
                .Select(m => new DriverSaccoMembershipDto
                {
                    Id = m.Id,
                    DriverId = m.DriverId,
                    SaccoId = m.SaccoId,
                    MembershipDate = m.MembershipDate,
                    ExpiryDate = m.ExpiryDate,
                    MembershipNumber = m.MembershipNumber,
                    Status = m.Status,
                    ShareContribution = m.ShareContribution,
                    MonthlyContribution = m.MonthlyContribution,
                    MembershipType = m.MembershipType,
                    Benefits = m.Benefits,
                    IsActive = m.IsActive,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    IsExpired = m.ExpiryDate.HasValue && m.ExpiryDate < DateTime.Today,
                    DriverName = m.Driver != null ? $"{m.Driver.FirstName} {m.Driver.LastName}".Trim() : null,
                    SaccoName = m.Sacco != null ? m.Sacco.Name : null
                })
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                return NotFound(ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Driver-SACCO membership not found"));
            }

            return Ok(ApiResponse<DriverSaccoMembershipDto>.SuccessResponse(membership));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Error retrieving driver-sacco membership", ex.Message));
        }
    }

    /// <summary>
    /// Create driver-sacco membership
    /// </summary>
    [HttpPost("driver-sacco-memberships")]
    public async Task<ActionResult<ApiResponse<DriverSaccoMembershipDto>>> CreateDriverSaccoMembership(CreateDriverSaccoMembershipDto dto)
    {
        try
        {
            var membership = new DriverSaccoMembership
            {
                Id = Guid.NewGuid(),
                DriverId = dto.DriverId,
                SaccoId = dto.SaccoId,
                MembershipDate = dto.MembershipDate,
                MembershipNumber = dto.MembershipNumber,
                ShareContribution = dto.ShareContribution,
                Status = dto.Status,
                ExpiryDate = dto.ExpiryDate,
                MonthlyContribution = dto.MonthlyContribution,
                MembershipType = dto.MembershipType,
                Benefits = dto.Benefits,
                Notes = dto.Notes
            };

            _context.DriverSaccoMemberships.Add(membership);
            await _context.SaveChangesAsync();

            var responseDto = new DriverSaccoMembershipDto
            {
                Id = membership.Id,
                DriverId = membership.DriverId,
                SaccoId = membership.SaccoId,
                MembershipDate = membership.MembershipDate,
                ExpiryDate = membership.ExpiryDate,
                MembershipNumber = membership.MembershipNumber,
                Status = membership.Status,
                ShareContribution = membership.ShareContribution,
                MonthlyContribution = membership.MonthlyContribution,
                MembershipType = membership.MembershipType,
                Benefits = membership.Benefits,
                IsActive = membership.IsActive,
                Notes = membership.Notes,
                CreatedAt = membership.CreatedAt,
                UpdatedAt = membership.UpdatedAt,
                IsExpired = membership.ExpiryDate.HasValue && membership.ExpiryDate < DateTime.Today,
                DriverName = "Driver Name", // TODO: Get from Driver entity via lookup
                SaccoName = "Sacco Name" // TODO: Get from Sacco entity via lookup
            };

            return CreatedAtAction(nameof(GetDriverSaccoMembership), 
                new { id = membership.Id }, 
                ApiResponse<DriverSaccoMembershipDto>.SuccessResponse(responseDto, "Driver-SACCO membership created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Error creating driver-sacco membership", ex.Message));
        }
    }

    /// <summary>
    /// Update driver-sacco membership
    /// </summary>
    [HttpPut("driver-sacco-memberships/{id}")]
    public async Task<ActionResult<ApiResponse<DriverSaccoMembershipDto>>> UpdateDriverSaccoMembership(Guid id, UpdateDriverSaccoMembershipDto dto)
    {
        try
        {
            var membership = await _context.DriverSaccoMemberships.FindAsync(id);
            if (membership == null)
            {
                return NotFound(ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Driver-SACCO membership not found"));
            }

            membership.MembershipDate = dto.MembershipDate;
            membership.MembershipNumber = dto.MembershipNumber;
            membership.Status = dto.Status;
            membership.ShareContribution = dto.ShareContribution;
            membership.ExpiryDate = dto.ExpiryDate;
            membership.MonthlyContribution = dto.MonthlyContribution;
            membership.MembershipType = dto.MembershipType;
            membership.Benefits = dto.Benefits;
            membership.IsActive = dto.IsActive;
            membership.Notes = dto.Notes;
            membership.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = new DriverSaccoMembershipDto
            {
                Id = membership.Id,
                DriverId = membership.DriverId,
                SaccoId = membership.SaccoId,
                MembershipDate = membership.MembershipDate,
                ExpiryDate = membership.ExpiryDate,
                MembershipNumber = membership.MembershipNumber,
                Status = membership.Status,
                ShareContribution = membership.ShareContribution,
                MonthlyContribution = membership.MonthlyContribution,
                MembershipType = membership.MembershipType,
                Benefits = membership.Benefits,
                IsActive = membership.IsActive,
                Notes = membership.Notes,
                CreatedAt = membership.CreatedAt,
                UpdatedAt = membership.UpdatedAt,
                IsExpired = membership.ExpiryDate.HasValue && membership.ExpiryDate < DateTime.Today,
                DriverName = "Driver Name", // TODO: Get from Driver entity via lookup
                SaccoName = "Sacco Name" // TODO: Get from Sacco entity via lookup
            };

            return Ok(ApiResponse<DriverSaccoMembershipDto>.SuccessResponse(responseDto, "Driver-SACCO membership updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Error updating driver-sacco membership", ex.Message));
        }
    }

    /// <summary>
    /// Partially update driver-sacco membership
    /// </summary>
    [HttpPatch("driver-sacco-memberships/{id}")]
    public async Task<ActionResult<ApiResponse<DriverSaccoMembershipDto>>> PatchDriverSaccoMembership(Guid id, PatchDriverSaccoMembershipDto dto)
    {
        try
        {
            var membership = await _context.DriverSaccoMemberships.FindAsync(id);
            if (membership == null)
            {
                return NotFound(ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Driver-SACCO membership not found"));
            }

            if (dto.MembershipDate.HasValue) membership.MembershipDate = dto.MembershipDate.Value;
            if (dto.MembershipNumber != null) membership.MembershipNumber = dto.MembershipNumber;
            if (dto.Status != null) membership.Status = dto.Status;
            if (dto.ShareContribution.HasValue) membership.ShareContribution = dto.ShareContribution.Value;
            if (dto.ExpiryDate.HasValue) membership.ExpiryDate = dto.ExpiryDate;
            if (dto.MonthlyContribution.HasValue) membership.MonthlyContribution = dto.MonthlyContribution;
            if (dto.MembershipType != null) membership.MembershipType = dto.MembershipType;
            if (dto.Benefits != null) membership.Benefits = dto.Benefits;
            if (dto.IsActive.HasValue) membership.IsActive = dto.IsActive.Value;
            if (dto.Notes != null) membership.Notes = dto.Notes;
            membership.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = new DriverSaccoMembershipDto
            {
                Id = membership.Id,
                DriverId = membership.DriverId,
                SaccoId = membership.SaccoId,
                MembershipDate = membership.MembershipDate,
                ExpiryDate = membership.ExpiryDate,
                MembershipNumber = membership.MembershipNumber,
                Status = membership.Status,
                ShareContribution = membership.ShareContribution,
                MonthlyContribution = membership.MonthlyContribution,
                MembershipType = membership.MembershipType,
                Benefits = membership.Benefits,
                IsActive = membership.IsActive,
                Notes = membership.Notes,
                CreatedAt = membership.CreatedAt,
                UpdatedAt = membership.UpdatedAt,
                IsExpired = membership.ExpiryDate.HasValue && membership.ExpiryDate < DateTime.Today,
                DriverName = "Driver Name", // TODO: Get from Driver entity via lookup
                SaccoName = "Sacco Name" // TODO: Get from Sacco entity via lookup
            };

            return Ok(ApiResponse<DriverSaccoMembershipDto>.SuccessResponse(responseDto, "Driver-SACCO membership updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Error updating driver-sacco membership", ex.Message));
        }
    }

    /// <summary>
    /// Delete driver-sacco membership
    /// </summary>
    [HttpDelete("driver-sacco-memberships/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriverSaccoMembership(Guid id)
    {
        try
        {
            var membership = await _context.DriverSaccoMemberships.FindAsync(id);
            if (membership == null)
            {
                return NotFound(ApiResponse.CreateError("Driver-SACCO membership not found"));
            }

            membership.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Driver-SACCO membership deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting driver-sacco membership", ex.Message));
        }
    }

    #endregion

    #region Driver-Vehicle Assignment

    /// <summary>
    /// Get driver-vehicle assignments with pagination
    /// </summary>
    [HttpGet("driver-vehicle-assignments")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverVehicleAssignmentDto>>>> GetDriverVehicleAssignments(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverVehicleAssignments
                .Include(a => a.Driver)
                .Include(a => a.Vehicle)
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
            return StatusCode(500, ApiResponse<PagedResult<DriverVehicleAssignmentDto>>.ErrorResponse("Error retrieving driver-vehicle assignments", ex.Message));
        }
    }

    /// <summary>
    /// Create driver-vehicle assignment
    /// </summary>
    [HttpPost("driver-vehicle-assignments")]
    public async Task<ActionResult<ApiResponse<DriverVehicleAssignmentDto>>> CreateDriverVehicleAssignment(CreateDriverVehicleAssignmentDto dto)
    {
        try
        {
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
                DriverName = "Driver Name", // TODO: Get from Driver entity via lookup
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Get from Vehicle entity via lookup
                IsCurrentlyAssigned = assignment.IsActive && assignment.UnassignedDate == null
            };

            return Ok(ApiResponse<DriverVehicleAssignmentDto>.SuccessResponse(responseDto, "Driver-vehicle assignment created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Error creating driver-vehicle assignment", ex.Message));
        }
    }

    /// <summary>
    /// Get specific driver-vehicle assignment
    /// </summary>
    [HttpGet("driver-vehicle-assignments/{id}")]
    public async Task<ActionResult<ApiResponse<DriverVehicleAssignmentDto>>> GetDriverVehicleAssignment(Guid id)
    {
        try
        {
            var assignment = await _context.DriverVehicleAssignments
                .Where(a => a.Id == id)
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
                    DriverName = "Driver Name", // TODO: Add join after entity creation
                    VehicleRegistrationNumber = "Vehicle Reg", // TODO: Add join after entity creation
                    IsCurrentlyAssigned = a.IsActive && a.UnassignedDate == null
                })
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                return NotFound(ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Driver-vehicle assignment not found"));
            }

            return Ok(ApiResponse<DriverVehicleAssignmentDto>.SuccessResponse(assignment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Error retrieving driver-vehicle assignment", ex.Message));
        }
    }

    /// <summary>
    /// Update driver-vehicle assignment
    /// </summary>
    [HttpPut("driver-vehicle-assignments/{id}")]
    public async Task<ActionResult<ApiResponse<DriverVehicleAssignmentDto>>> UpdateDriverVehicleAssignment(Guid id, UpdateDriverVehicleAssignmentDto dto)
    {
        try
        {
            var assignment = await _context.DriverVehicleAssignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound(ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Driver-vehicle assignment not found"));
            }

            assignment.AssignedDate = dto.AssignedDate;
            assignment.IsPrimary = dto.IsPrimary;
            assignment.AssignmentType = dto.AssignmentType;
            assignment.AssignedBy = dto.AssignedBy;
            assignment.UnassignedDate = dto.UnassignedDate;
            assignment.Reason = dto.Reason;
            assignment.UnassignedBy = dto.UnassignedBy;
            assignment.IsActive = dto.IsActive;
            assignment.Notes = dto.Notes;
            assignment.UpdatedAt = DateTime.UtcNow;

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
                DriverName = "Driver Name", // TODO: Get from Driver entity via lookup
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Get from Vehicle entity via lookup
                IsCurrentlyAssigned = assignment.IsActive && assignment.UnassignedDate == null
            };

            return Ok(ApiResponse<DriverVehicleAssignmentDto>.SuccessResponse(responseDto, "Driver-vehicle assignment updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverVehicleAssignmentDto>.ErrorResponse("Error updating driver-vehicle assignment", ex.Message));
        }
    }

    /// <summary>
    /// Delete driver-vehicle assignment
    /// </summary>
    [HttpDelete("driver-vehicle-assignments/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriverVehicleAssignment(Guid id)
    {
        try
        {
            var assignment = await _context.DriverVehicleAssignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound(ApiResponse.CreateError("Driver-vehicle assignment not found"));
            }

            assignment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Driver-vehicle assignment deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting driver-vehicle assignment", ex.Message));
        }
    }

    #endregion

    #region Vehicle-Transporter Ownership

    /// <summary>
    /// Get vehicle-transporter ownerships with pagination
    /// </summary>
    [HttpGet("vehicle-transporter-ownerships")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleTransporterOwnershipDto>>>> GetVehicleTransporterOwnerships(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleTransporterOwnerships
                .Include(o => o.Vehicle)
                .Include(o => o.TransporterProfile)
                    .ThenInclude(tp => tp.BusinessEntity)
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
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleTransporterOwnershipDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleTransporterOwnershipDto>>.ErrorResponse("Error retrieving vehicle-transporter ownerships", ex.Message));
        }
    }

    /// <summary>
    /// Get specific vehicle-transporter ownership
    /// </summary>
    [HttpGet("vehicle-transporter-ownerships/{id}")]
    public async Task<ActionResult<ApiResponse<VehicleTransporterOwnershipDto>>> GetVehicleTransporterOwnership(Guid id)
    {
        try
        {
            var ownership = await _context.VehicleTransporterOwnerships
                .Where(o => o.Id == id)
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
                return NotFound(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle-transporter ownership not found"));
            }

            return Ok(ApiResponse<VehicleTransporterOwnershipDto>.SuccessResponse(ownership));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Error retrieving vehicle-transporter ownership", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle-transporter ownership
    /// </summary>
    [HttpPost("vehicle-transporter-ownerships")]
    public async Task<ActionResult<ApiResponse<VehicleTransporterOwnershipDto>>> CreateVehicleTransporterOwnership(CreateVehicleTransporterOwnershipDto dto)
    {
        try
        {
            var ownership = new VehicleTransporterOwnership
            {
                Id = Guid.NewGuid(),
                VehicleId = dto.VehicleId,
                TransporterProfileId = dto.TransporterProfileId,
                OwnershipStartDate = dto.OwnershipStartDate,
                OwnershipType = dto.OwnershipType,
                OwnershipEndDate = dto.OwnershipEndDate,
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
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Get from Vehicle entity via lookup
                TransporterName = "Transporter Name" // TODO: Get from Transporter entity via lookup
            };

            return CreatedAtAction(nameof(GetVehicleTransporterOwnership), 
                new { id = ownership.Id }, 
                ApiResponse<VehicleTransporterOwnershipDto>.SuccessResponse(responseDto, "Vehicle-transporter ownership created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Error creating vehicle-transporter ownership", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle-transporter ownership
    /// </summary>
    [HttpPut("vehicle-transporter-ownerships/{id}")]
    public async Task<ActionResult<ApiResponse<VehicleTransporterOwnershipDto>>> UpdateVehicleTransporterOwnership(Guid id, UpdateVehicleTransporterOwnershipDto dto)
    {
        try
        {
            var ownership = await _context.VehicleTransporterOwnerships.FindAsync(id);
            if (ownership == null)
            {
                return NotFound(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle-transporter ownership not found"));
            }

            ownership.OwnershipStartDate = dto.OwnershipStartDate;
            ownership.OwnershipType = dto.OwnershipType;
            ownership.OwnershipEndDate = dto.OwnershipEndDate;
            ownership.PurchasePrice = dto.PurchasePrice;
            ownership.CurrentValue = dto.CurrentValue;
            ownership.FinancingDetails = dto.FinancingDetails;
            ownership.InsuranceDetails = dto.InsuranceDetails;
            ownership.IsActive = dto.IsActive;
            ownership.Notes = dto.Notes;
            ownership.UpdatedAt = DateTime.UtcNow;

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
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Get from Vehicle entity via lookup
                TransporterName = "Transporter Name" // TODO: Get from Transporter entity via lookup
            };

            return Ok(ApiResponse<VehicleTransporterOwnershipDto>.SuccessResponse(responseDto, "Vehicle-transporter ownership updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Error updating vehicle-transporter ownership", ex.Message));
        }
    }

    /// <summary>
    /// Partially update vehicle-transporter ownership
    /// </summary>
    [HttpPatch("vehicle-transporter-ownerships/{id}")]
    public async Task<ActionResult<ApiResponse<VehicleTransporterOwnershipDto>>> PatchVehicleTransporterOwnership(Guid id, PatchVehicleTransporterOwnershipDto dto)
    {
        try
        {
            var ownership = await _context.VehicleTransporterOwnerships.FindAsync(id);
            if (ownership == null)
            {
                return NotFound(ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Vehicle-transporter ownership not found"));
            }

            if (dto.OwnershipStartDate.HasValue) ownership.OwnershipStartDate = dto.OwnershipStartDate.Value;
            if (dto.OwnershipType != null) ownership.OwnershipType = dto.OwnershipType;
            if (dto.OwnershipEndDate.HasValue) ownership.OwnershipEndDate = dto.OwnershipEndDate;
            if (dto.PurchasePrice.HasValue) ownership.PurchasePrice = dto.PurchasePrice;
            if (dto.CurrentValue.HasValue) ownership.CurrentValue = dto.CurrentValue;
            if (dto.FinancingDetails != null) ownership.FinancingDetails = dto.FinancingDetails;
            if (dto.InsuranceDetails != null) ownership.InsuranceDetails = dto.InsuranceDetails;
            if (dto.IsActive.HasValue) ownership.IsActive = dto.IsActive.Value;
            if (dto.Notes != null) ownership.Notes = dto.Notes;
            ownership.UpdatedAt = DateTime.UtcNow;

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
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Get from Vehicle entity via lookup
                TransporterName = "Transporter Name" // TODO: Get from Transporter entity via lookup
            };

            return Ok(ApiResponse<VehicleTransporterOwnershipDto>.SuccessResponse(responseDto, "Vehicle-transporter ownership updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleTransporterOwnershipDto>.ErrorResponse("Error updating vehicle-transporter ownership", ex.Message));
        }
    }

    /// <summary>
    /// Delete vehicle-transporter ownership
    /// </summary>
    [HttpDelete("vehicle-transporter-ownerships/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleTransporterOwnership(Guid id)
    {
        try
        {
            var ownership = await _context.VehicleTransporterOwnerships.FindAsync(id);
            if (ownership == null)
            {
                return NotFound(ApiResponse.CreateError("Vehicle-transporter ownership not found"));
            }

            ownership.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Vehicle-transporter ownership deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting vehicle-transporter ownership", ex.Message));
        }
    }

    #endregion

    #region Driver-Transporter Employment

    /// <summary>
    /// Get driver-transporter employments with pagination
    /// </summary>
    [HttpGet("driver-transporter-employments")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverTransporterEmploymentDto>>>> GetDriverTransporterEmployments(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverTransporterEmployments
                .Include(e => e.Driver)
                .Include(e => e.TransporterProfile)
                    .ThenInclude(tp => tp.BusinessEntity)
                .Select(e => new DriverTransporterEmploymentDto
                {
                    Id = e.Id,
                    DriverId = e.DriverId,
                    TransporterProfileId = e.TransporterProfileId,
                    HireDate = e.HireDate,
                    TerminationDate = e.TerminationDate,
                    EmploymentType = e.EmploymentType,
                    Status = e.Status,
                    Salary = e.Salary,
                    Position = e.Position,
                    Department = e.Department,
                    ReportsTo = e.ReportsTo,
                    TerminationReason = e.TerminationReason,
                    IsActive = e.IsActive,
                    Notes = e.Notes,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    DriverName = "Driver Name", // TODO: Join with Driver entity
                    TransporterName = "Transporter Name", // TODO: Join with Transporter entity
                    IsCurrentEmployment = e.IsActive && e.TerminationDate == null
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverTransporterEmploymentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverTransporterEmploymentDto>>.ErrorResponse("Error retrieving driver-transporter employments", ex.Message));
        }
    }

    /// <summary>
    /// Create driver-transporter employment
    /// </summary>
    [HttpPost("driver-transporter-employments")]
    public async Task<ActionResult<ApiResponse<DriverTransporterEmploymentDto>>> CreateDriverTransporterEmployment(CreateDriverTransporterEmploymentDto dto)
    {
        try
        {
            var employment = new DriverTransporterEmployment
            {
                Id = Guid.NewGuid(),
                DriverId = dto.DriverId,
                TransporterProfileId = dto.TransporterProfileId,
                HireDate = dto.HireDate,
                EmploymentType = dto.EmploymentType,
                Status = dto.Status,
                TerminationDate = dto.TerminationDate,
                Salary = dto.Salary,
                Position = dto.Position,
                Department = dto.Department,
                ReportsTo = dto.ReportsTo,
                Notes = dto.Notes
            };

            _context.DriverTransporterEmployments.Add(employment);
            await _context.SaveChangesAsync();

            var responseDto = new DriverTransporterEmploymentDto
            {
                Id = employment.Id,
                DriverId = employment.DriverId,
                TransporterProfileId = employment.TransporterProfileId,
                HireDate = employment.HireDate,
                TerminationDate = employment.TerminationDate,
                EmploymentType = employment.EmploymentType,
                Status = employment.Status,
                Salary = employment.Salary,
                Position = employment.Position,
                Department = employment.Department,
                ReportsTo = employment.ReportsTo,
                TerminationReason = employment.TerminationReason,
                IsActive = employment.IsActive,
                Notes = employment.Notes,
                CreatedAt = employment.CreatedAt,
                UpdatedAt = employment.UpdatedAt,
                DriverName = "Driver Name", // TODO: Get from Driver entity via lookup
                TransporterName = "Transporter Name", // TODO: Get from Transporter entity via lookup
                IsCurrentEmployment = employment.IsActive && employment.TerminationDate == null
            };

            return Ok(ApiResponse<DriverTransporterEmploymentDto>.SuccessResponse(responseDto, "Driver-transporter employment created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverTransporterEmploymentDto>.ErrorResponse("Error creating driver-transporter employment", ex.Message));
        }
    }

    #endregion

    #region Product-Supplier Catalog

    /// <summary>
    /// Get product-supplier catalogs with pagination
    /// </summary>
    [HttpGet("product-supplier-catalogs")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSupplierCatalogDto>>>> GetProductSupplierCatalogs(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ProductSupplierCatalogs
                .Include(c => c.Product)
                .Include(c => c.SupplierProfile)
                    .ThenInclude(sp => sp.BusinessEntity)
                .Select(c => new ProductSupplierCatalogDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    SupplierProfileId = c.SupplierProfileId,
                    UnitPrice = c.UnitPrice,
                    Currency = c.Currency,
                    MinOrderQuantity = c.MinOrderQuantity,
                    MaxOrderQuantity = c.MaxOrderQuantity,
                    LeadTimeDays = c.LeadTimeDays,
                    IsPreferred = c.IsPreferred,
                    ValidFrom = c.ValidFrom,
                    ValidTo = c.ValidTo,
                    ProductCode = c.ProductCode,
                    Description = c.Description,
                    QualityRating = c.QualityRating,
                    Discount = c.Discount,
                    PaymentTerms = c.PaymentTerms,
                    IsActive = c.IsActive,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductName = "Product Name", // TODO: Join with Product entity
                    SupplierName = "Supplier Name", // TODO: Join with Supplier entity
                    IsCurrentlyValid = c.ValidFrom <= DateTime.Today && (c.ValidTo == null || c.ValidTo >= DateTime.Today)
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ProductSupplierCatalogDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ProductSupplierCatalogDto>>.ErrorResponse("Error retrieving product-supplier catalogs", ex.Message));
        }
    }

    /// <summary>
    /// Create product-supplier catalog
    /// </summary>
    [HttpPost("product-supplier-catalogs")]
    public async Task<ActionResult<ApiResponse<ProductSupplierCatalogDto>>> CreateProductSupplierCatalog(CreateProductSupplierCatalogDto dto)
    {
        try
        {
            var catalog = new ProductSupplierCatalog
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                SupplierProfileId = dto.SupplierProfileId,
                UnitPrice = dto.UnitPrice,
                ValidFrom = dto.ValidFrom,
                Currency = dto.Currency,
                MinOrderQuantity = dto.MinOrderQuantity,
                MaxOrderQuantity = dto.MaxOrderQuantity,
                LeadTimeDays = dto.LeadTimeDays,
                IsPreferred = dto.IsPreferred,
                ValidTo = dto.ValidTo,
                ProductCode = dto.ProductCode,
                Description = dto.Description,
                QualityRating = dto.QualityRating,
                Discount = dto.Discount,
                PaymentTerms = dto.PaymentTerms,
                Notes = dto.Notes
            };

            _context.ProductSupplierCatalogs.Add(catalog);
            await _context.SaveChangesAsync();

            var responseDto = new ProductSupplierCatalogDto
            {
                Id = catalog.Id,
                ProductId = catalog.ProductId,
                SupplierProfileId = catalog.SupplierProfileId,
                UnitPrice = catalog.UnitPrice,
                Currency = catalog.Currency,
                MinOrderQuantity = catalog.MinOrderQuantity,
                MaxOrderQuantity = catalog.MaxOrderQuantity,
                LeadTimeDays = catalog.LeadTimeDays,
                IsPreferred = catalog.IsPreferred,
                ValidFrom = catalog.ValidFrom,
                ValidTo = catalog.ValidTo,
                ProductCode = catalog.ProductCode,
                Description = catalog.Description,
                QualityRating = catalog.QualityRating,
                Discount = catalog.Discount,
                PaymentTerms = catalog.PaymentTerms,
                IsActive = catalog.IsActive,
                Notes = catalog.Notes,
                CreatedAt = catalog.CreatedAt,
                UpdatedAt = catalog.UpdatedAt,
                ProductName = "Product Name", // TODO: Get from Product entity via lookup
                SupplierName = "Supplier Name", // TODO: Get from Supplier entity via lookup
                IsCurrentlyValid = catalog.ValidFrom <= DateTime.Today && (catalog.ValidTo == null || catalog.ValidTo >= DateTime.Today)
            };

            return Ok(ApiResponse<ProductSupplierCatalogDto>.SuccessResponse(responseDto, "Product-supplier catalog created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductSupplierCatalogDto>.ErrorResponse("Error creating product-supplier catalog", ex.Message));
        }
    }

    #endregion

    #region Route-Weighbridge Association

    /// <summary>
    /// Get route-weighbridge associations with pagination
    /// </summary>
    [HttpGet("route-weighbridge-associations")]
    public async Task<ActionResult<ApiResponse<PagedResult<RouteWeighbridgeAssociationDto>>>> GetRouteWeighbridgeAssociations(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.RouteWeighbridgeAssociations
                .Include(a => a.Route)
                .Include(a => a.Weighbridge)
                .Select(a => new RouteWeighbridgeAssociationDto
                {
                    Id = a.Id,
                    RouteId = a.RouteId,
                    WeighbridgeId = a.WeighbridgeId,
                    AssociationType = a.AssociationType,
                    SequenceOrder = a.SequenceOrder,
                    IsMandatory = a.IsMandatory,
                    IsActive = a.IsActive,
                    Instructions = a.Instructions,
                    EstimatedDurationMinutes = a.EstimatedDurationMinutes,
                    Distance = a.Distance,
                    Conditions = a.Conditions,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    RouteName = "Route Name", // TODO: Join with Route entity
                    WeighbridgeName = "Weighbridge Name" // TODO: Join with Weighbridge entity
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<RouteWeighbridgeAssociationDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<RouteWeighbridgeAssociationDto>>.ErrorResponse("Error retrieving route-weighbridge associations", ex.Message));
        }
    }

    /// <summary>
    /// Create route-weighbridge association
    /// </summary>
    [HttpPost("route-weighbridge-associations")]
    public async Task<ActionResult<ApiResponse<RouteWeighbridgeAssociationDto>>> CreateRouteWeighbridgeAssociation(CreateRouteWeighbridgeAssociationDto dto)
    {
        try
        {
            var association = new RouteWeighbridgeAssociation
            {
                Id = Guid.NewGuid(),
                RouteId = dto.RouteId,
                WeighbridgeId = dto.WeighbridgeId,
                SequenceOrder = dto.SequenceOrder,
                AssociationType = dto.AssociationType,
                IsMandatory = dto.IsMandatory,
                Instructions = dto.Instructions,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                Distance = dto.Distance,
                Conditions = dto.Conditions,
                Notes = dto.Notes
            };

            _context.RouteWeighbridgeAssociations.Add(association);
            await _context.SaveChangesAsync();

            var responseDto = new RouteWeighbridgeAssociationDto
            {
                Id = association.Id,
                RouteId = association.RouteId,
                WeighbridgeId = association.WeighbridgeId,
                AssociationType = association.AssociationType,
                SequenceOrder = association.SequenceOrder,
                IsMandatory = association.IsMandatory,
                IsActive = association.IsActive,
                Instructions = association.Instructions,
                EstimatedDurationMinutes = association.EstimatedDurationMinutes,
                Distance = association.Distance,
                Conditions = association.Conditions,
                Notes = association.Notes,
                CreatedAt = association.CreatedAt,
                UpdatedAt = association.UpdatedAt,
                RouteName = "Route Name", // TODO: Get from Route entity via lookup
                WeighbridgeName = "Weighbridge Name" // TODO: Get from Weighbridge entity via lookup
            };

            return Ok(ApiResponse<RouteWeighbridgeAssociationDto>.SuccessResponse(responseDto, "Route-weighbridge association created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteWeighbridgeAssociationDto>.ErrorResponse("Error creating route-weighbridge association", ex.Message));
        }
    }

    #endregion

    #region Vehicle-SACCO Registration

    /// <summary>
    /// Get vehicle-sacco registrations with pagination
    /// </summary>
    [HttpGet("vehicle-sacco-registrations")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>>> GetVehicleSaccoRegistrations(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleSaccoRegistrations
                .Include(r => r.Vehicle)
                .Include(r => r.Sacco)
                .Select(r => new VehicleSaccoRegistrationDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    SaccoId = r.SaccoId,
                    RegistrationDate = r.RegistrationDate,
                    ExpiryDate = r.ExpiryDate,
                    RegistrationNumber = r.RegistrationNumber,
                    Status = r.Status,
                    RegistrationFee = r.RegistrationFee,
                    CertificateNumber = r.CertificateNumber,
                    Conditions = r.Conditions,
                    IsActive = r.IsActive,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    IsExpired = r.ExpiryDate.HasValue && r.ExpiryDate < DateTime.Today,
                    VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity
                    SaccoName = "Sacco Name" // TODO: Join with Sacco entity
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>.ErrorResponse("Error retrieving vehicle-sacco registrations", ex.Message));
        }
    }

    /// <summary>
    /// Create vehicle-sacco registration
    /// </summary>
    [HttpPost("vehicle-sacco-registrations")]
    public async Task<ActionResult<ApiResponse<VehicleSaccoRegistrationDto>>> CreateVehicleSaccoRegistration(CreateVehicleSaccoRegistrationDto dto)
    {
        try
        {
            var registration = new VehicleSaccoRegistration
            {
                Id = Guid.NewGuid(),
                VehicleId = dto.VehicleId,
                SaccoId = dto.SaccoId,
                RegistrationDate = dto.RegistrationDate,
                RegistrationNumber = dto.RegistrationNumber,
                RegistrationFee = dto.RegistrationFee,
                Status = dto.Status,
                ExpiryDate = dto.ExpiryDate,
                CertificateNumber = dto.CertificateNumber,
                Conditions = dto.Conditions,
                Notes = dto.Notes
            };

            _context.VehicleSaccoRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            var responseDto = new VehicleSaccoRegistrationDto
            {
                Id = registration.Id,
                VehicleId = registration.VehicleId,
                SaccoId = registration.SaccoId,
                RegistrationDate = registration.RegistrationDate,
                ExpiryDate = registration.ExpiryDate,
                RegistrationNumber = registration.RegistrationNumber,
                Status = registration.Status,
                RegistrationFee = registration.RegistrationFee,
                CertificateNumber = registration.CertificateNumber,
                Conditions = registration.Conditions,
                IsActive = registration.IsActive,
                Notes = registration.Notes,
                CreatedAt = registration.CreatedAt,
                UpdatedAt = registration.UpdatedAt,
                IsExpired = registration.ExpiryDate.HasValue && registration.ExpiryDate < DateTime.Today,
                VehicleRegistrationNumber = "Vehicle Reg", // TODO: Get from Vehicle entity via lookup
                SaccoName = "Sacco Name" // TODO: Get from Sacco entity via lookup
            };

            return Ok(ApiResponse<VehicleSaccoRegistrationDto>.SuccessResponse(responseDto, "Vehicle-sacco registration created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<VehicleSaccoRegistrationDto>.ErrorResponse("Error creating vehicle-sacco registration", ex.Message));
        }
    }

    #endregion

    #region Convenience endpoints for cross-module queries

    /// <summary>
    /// Get all vehicles owned by a specific transporter
    /// </summary>
    [HttpGet("transporters/{transporterId}/vehicles")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleTransporterOwnershipDto>>>> GetTransporterVehicles(
        Guid transporterId, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.VehicleTransporterOwnerships
                .Where(o => o.TransporterProfileId == transporterId && o.IsActive)
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
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleTransporterOwnershipDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleTransporterOwnershipDto>>.ErrorResponse("Error retrieving transporter vehicles", ex.Message));
        }
    }

    /// <summary>
    /// Get all drivers who are members of a specific SACCO
    /// </summary>
    [HttpGet("saccos/{saccoId}/drivers")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverSaccoMembershipDto>>>> GetSaccoDrivers(
        Guid saccoId, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverSaccoMemberships
                .Where(m => m.SaccoId == saccoId && m.IsActive)
                .Select(m => new DriverSaccoMembershipDto
                {
                    Id = m.Id,
                    DriverId = m.DriverId,
                    SaccoId = m.SaccoId,
                    MembershipDate = m.MembershipDate,
                    ExpiryDate = m.ExpiryDate,
                    MembershipNumber = m.MembershipNumber,
                    Status = m.Status,
                    ShareContribution = m.ShareContribution,
                    MonthlyContribution = m.MonthlyContribution,
                    MembershipType = m.MembershipType,
                    Benefits = m.Benefits,
                    IsActive = m.IsActive,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    IsExpired = m.ExpiryDate.HasValue && m.ExpiryDate < DateTime.Today,
                    DriverName = "Driver Name", // TODO: Join with Driver entity
                    SaccoName = "Sacco Name" // TODO: Join with Sacco entity
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverSaccoMembershipDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverSaccoMembershipDto>>.ErrorResponse("Error retrieving SACCO drivers", ex.Message));
        }
    }

    #endregion
}