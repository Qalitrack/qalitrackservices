using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Weighbridge.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Weighbridge Module")]
public class WeighbridgeController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public WeighbridgeController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get weighbridges with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of weighbridges</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Weighbridge>>>> GetWeighbridges(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Weighbridges
                .Include(w => w.Calibrations)
                .Include(w => w.MaintenanceRecords)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<Weighbridge>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Weighbridge>>.ErrorResponse("Error retrieving weighbridges", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Weighbridge>>> GetWeighbridge(Guid id)
    {
        try
        {
            var weighbridge = await _context.Weighbridges
                .Include(w => w.Calibrations.OrderByDescending(c => c.CalibrationDate))
                .Include(w => w.MaintenanceRecords.OrderByDescending(m => m.MaintenanceDate))
                .Include(w => w.Transactions.OrderByDescending(t => t.TransactionDate).Take(50))
                .Include(w => w.Documents)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weighbridge == null)
            {
                return NotFound(ApiResponse<Weighbridge>.ErrorResponse("Weighbridge not found"));
            }

            return Ok(ApiResponse<Weighbridge>.SuccessResponse(weighbridge));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Weighbridge>.ErrorResponse("Error retrieving weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Create new weighbridge
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Weighbridge>>> CreateWeighbridge(Weighbridge weighbridge)
    {
        try
        {
            _context.Weighbridges.Add(weighbridge);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWeighbridge), 
                new { id = weighbridge.Id }, 
                ApiResponse<Weighbridge>.SuccessResponse(weighbridge, "Weighbridge created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Weighbridge>.ErrorResponse("Error creating weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Update weighbridge
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Weighbridge>>> UpdateWeighbridge(Guid id, Weighbridge weighbridge)
    {
        if (id != weighbridge.Id)
        {
            return BadRequest(ApiResponse<Weighbridge>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(weighbridge).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Weighbridge>.SuccessResponse(weighbridge, "Weighbridge updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await WeighbridgeExists(id))
            {
                return NotFound(ApiResponse<Weighbridge>.ErrorResponse("Weighbridge not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Weighbridge>.ErrorResponse("Error updating weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Delete weighbridge (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridge(Guid id)
    {
        try
        {
            var weighbridge = await _context.Weighbridges.FindAsync(id);
            if (weighbridge == null)
            {
                return NotFound(ApiResponse.CreateError("Weighbridge not found"));
            }

            weighbridge.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Weighbridge deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge transactions
    /// </summary>
    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<ApiResponse<PagedResult<WeighbridgeTransaction>>>> GetWeighbridgeTransactions(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.WeighbridgeTransactions
                .Where(t => t.WeighbridgeId == id)
                .Include(t => t.Weighbridge)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<WeighbridgeTransaction>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<WeighbridgeTransaction>>.ErrorResponse("Error retrieving weighbridge transactions", ex.Message));
        }
    }

    /// <summary>
    /// Create weighbridge transaction
    /// </summary>
    [HttpPost("{id}/transactions")]
    public async Task<ActionResult<ApiResponse<WeighbridgeTransaction>>> CreateTransaction(Guid id, WeighbridgeTransaction transaction)
    {
        try
        {
            transaction.WeighbridgeId = id;
            _context.WeighbridgeTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<WeighbridgeTransaction>.SuccessResponse(transaction, "Transaction created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeTransaction>.ErrorResponse("Error creating transaction", ex.Message));
        }
    }

    private async Task<bool> WeighbridgeExists(Guid id)
    {
        return await _context.Weighbridges.AnyAsync(e => e.Id == id);
    }
}