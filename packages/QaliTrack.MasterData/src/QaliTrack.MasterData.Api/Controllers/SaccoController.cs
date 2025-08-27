using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Sacco.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("SACCO Module")]
public class SaccoController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public SaccoController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get SACCOs with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of SACCOs</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Sacco>>>> GetSaccos(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Saccos
                .Include(s => s.Financial)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<Sacco>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Sacco>>.ErrorResponse("Error retrieving SACCOs", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Sacco>>> GetSacco(Guid id)
    {
        try
        {
            var sacco = await _context.Saccos
                .Include(s => s.Members)
                .Include(s => s.Committees)
                .Include(s => s.Financial)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sacco == null)
            {
                return NotFound(ApiResponse<Sacco>.ErrorResponse("SACCO not found"));
            }

            return Ok(ApiResponse<Sacco>.SuccessResponse(sacco));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Sacco>.ErrorResponse("Error retrieving SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Create new SACCO
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Sacco>>> CreateSacco(Sacco sacco)
    {
        try
        {
            _context.Saccos.Add(sacco);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSacco), 
                new { id = sacco.Id }, 
                ApiResponse<Sacco>.SuccessResponse(sacco, "SACCO created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Sacco>.ErrorResponse("Error creating SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Update SACCO
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Sacco>>> UpdateSacco(Guid id, Sacco sacco)
    {
        if (id != sacco.Id)
        {
            return BadRequest(ApiResponse<Sacco>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(sacco).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Sacco>.SuccessResponse(sacco, "SACCO updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await SaccoExists(id))
            {
                return NotFound(ApiResponse<Sacco>.ErrorResponse("SACCO not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Sacco>.ErrorResponse("Error updating SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Delete SACCO (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSacco(Guid id)
    {
        try
        {
            var sacco = await _context.Saccos.FindAsync(id);
            if (sacco == null)
            {
                return NotFound(ApiResponse.CreateError("SACCO not found"));
            }

            sacco.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("SACCO deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting SACCO", ex.Message));
        }
    }

    private async Task<bool> SaccoExists(Guid id)
    {
        return await _context.Saccos.AnyAsync(e => e.Id == id);
    }
}