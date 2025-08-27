using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
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
    public async Task<ActionResult<ApiResponse<PagedResult<Vehicle>>>> GetVehicles(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Registration)
                .Include(v => v.Specification)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<Vehicle>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Vehicle>>.ErrorResponse("Error retrieving vehicles", ex.Message));
        }
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Vehicle>>> GetVehicle(Guid id)
    {
        try
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Registration)
                .Include(v => v.Specification)
                .Include(v => v.Documents)
                .Include(v => v.Inspections)
                .Include(v => v.InsurancePolicies)
                .Include(v => v.MaintenanceRecords)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound(ApiResponse<Vehicle>.ErrorResponse("Vehicle not found"));
            }

            return Ok(ApiResponse<Vehicle>.SuccessResponse(vehicle));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Vehicle>.ErrorResponse("Error retrieving vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Create new vehicle
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Vehicle>>> CreateVehicle(Vehicle vehicle)
    {
        try
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), 
                new { id = vehicle.Id }, 
                ApiResponse<Vehicle>.SuccessResponse(vehicle, "Vehicle created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Vehicle>.ErrorResponse("Error creating vehicle", ex.Message));
        }
    }

    /// <summary>
    /// Update vehicle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Vehicle>>> UpdateVehicle(Guid id, Vehicle vehicle)
    {
        if (id != vehicle.Id)
        {
            return BadRequest(ApiResponse<Vehicle>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(vehicle).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Vehicle>.SuccessResponse(vehicle, "Vehicle updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await VehicleExists(id))
            {
                return NotFound(ApiResponse<Vehicle>.ErrorResponse("Vehicle not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Vehicle>.ErrorResponse("Error updating vehicle", ex.Message));
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

    private async Task<bool> VehicleExists(Guid id)
    {
        return await _context.Vehicles.AnyAsync(e => e.Id == id);
    }
}