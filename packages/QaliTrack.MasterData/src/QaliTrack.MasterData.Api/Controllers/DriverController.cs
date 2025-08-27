using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Driver Module")]
public class DriverController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public DriverController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all drivers for an organization
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Driver>>>> GetDrivers([FromQuery] Guid? organizationId = null)
    {
        try
        {
            var query = _context.Drivers
                .Include(d => d.License)
                .Include(d => d.Profile)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                query = query.Where(d => d.OrganizationId == organizationId.Value);
            }

            var drivers = await query.ToListAsync();

            return Ok(ApiResponse<IEnumerable<Driver>>.SuccessResponse(drivers));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<Driver>>.ErrorResponse("Error retrieving drivers", ex.Message));
        }
    }

    /// <summary>
    /// Get driver by ID with all related data
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Driver>>> GetDriver(Guid id)
    {
        try
        {
            var driver = await _context.Drivers
                .Include(d => d.License)
                .Include(d => d.Profile)
                .Include(d => d.Documents)
                .Include(d => d.Trainings)
                .Include(d => d.MedicalRecords)
                .Include(d => d.Violations)
                .Include(d => d.PerformanceRecords)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
            {
                return NotFound(ApiResponse<Driver>.ErrorResponse("Driver not found"));
            }

            return Ok(ApiResponse<Driver>.SuccessResponse(driver));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Driver>.ErrorResponse("Error retrieving driver", ex.Message));
        }
    }

    /// <summary>
    /// Create new driver
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Driver>>> CreateDriver(Driver driver)
    {
        try
        {
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDriver), 
                new { id = driver.Id }, 
                ApiResponse<Driver>.SuccessResponse(driver, "Driver created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Driver>.ErrorResponse("Error creating driver", ex.Message));
        }
    }

    /// <summary>
    /// Update driver
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Driver>>> UpdateDriver(Guid id, Driver driver)
    {
        if (id != driver.Id)
        {
            return BadRequest(ApiResponse<Driver>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(driver).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Driver>.SuccessResponse(driver, "Driver updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await DriverExists(id))
            {
                return NotFound(ApiResponse<Driver>.ErrorResponse("Driver not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Driver>.ErrorResponse("Error updating driver", ex.Message));
        }
    }

    /// <summary>
    /// Get driver's SACCO memberships (cross-module relationship)
    /// </summary>
    [HttpGet("{id}/sacco-memberships")]
    public async Task<ActionResult<ApiResponse<object>>> GetDriverSaccoMemberships(Guid id)
    {
        try
        {
            var memberships = await _context.DriverSaccoMemberships
                .Where(dsm => dsm.DriverId == id && dsm.IsActive)
                .ToListAsync();

            return Ok(ApiResponse<object>.SuccessResponse(memberships));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error retrieving SACCO memberships", ex.Message));
        }
    }

    /// <summary>
    /// Get driver's current vehicle assignments (cross-module relationship)
    /// </summary>
    [HttpGet("{id}/vehicle-assignments")]
    public async Task<ActionResult<ApiResponse<object>>> GetDriverVehicleAssignments(Guid id)
    {
        try
        {
            var assignments = await _context.DriverVehicleAssignments
                .Where(dva => dva.DriverId == id && dva.IsActive)
                .ToListAsync();

            return Ok(ApiResponse<object>.SuccessResponse(assignments));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error retrieving vehicle assignments", ex.Message));
        }
    }

    private async Task<bool> DriverExists(Guid id)
    {
        return await _context.Drivers.AnyAsync(e => e.Id == id);
    }
}