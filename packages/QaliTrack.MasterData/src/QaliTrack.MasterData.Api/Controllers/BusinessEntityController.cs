using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("BusinessEntity Module")]
public class BusinessEntityController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public BusinessEntityController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get business entities with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of business entities</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BusinessEntity>>>> GetBusinessEntities(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.BusinessEntities
                .Include(be => be.CustomerProfile)
                .Include(be => be.SupplierProfile)
                .Include(be => be.TransporterProfile)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<BusinessEntity>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<BusinessEntity>>.ErrorResponse("Error retrieving business entities", ex.Message));
        }
    }

    /// <summary>
    /// Get business entity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BusinessEntity>>> GetBusinessEntity(Guid id)
    {
        try
        {
            var businessEntity = await _context.BusinessEntities
                .Include(be => be.CustomerProfile)
                .Include(be => be.SupplierProfile)
                .Include(be => be.TransporterProfile)
                .Include(be => be.Contacts)
                .Include(be => be.Locations)
                .Include(be => be.Documents)
                .FirstOrDefaultAsync(be => be.Id == id);

            if (businessEntity == null)
            {
                return NotFound(ApiResponse<BusinessEntity>.ErrorResponse("Business entity not found"));
            }

            return Ok(ApiResponse<BusinessEntity>.SuccessResponse(businessEntity));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BusinessEntity>.ErrorResponse("Error retrieving business entity", ex.Message));
        }
    }

    /// <summary>
    /// Create new business entity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BusinessEntity>>> CreateBusinessEntity(BusinessEntity businessEntity)
    {
        try
        {
            _context.BusinessEntities.Add(businessEntity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBusinessEntity), 
                new { id = businessEntity.Id }, 
                ApiResponse<BusinessEntity>.SuccessResponse(businessEntity, "Business entity created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BusinessEntity>.ErrorResponse("Error creating business entity", ex.Message));
        }
    }

    /// <summary>
    /// Update business entity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BusinessEntity>>> UpdateBusinessEntity(Guid id, BusinessEntity businessEntity)
    {
        if (id != businessEntity.Id)
        {
            return BadRequest(ApiResponse<BusinessEntity>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(businessEntity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<BusinessEntity>.SuccessResponse(businessEntity, "Business entity updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BusinessEntityExists(id))
            {
                return NotFound(ApiResponse<BusinessEntity>.ErrorResponse("Business entity not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BusinessEntity>.ErrorResponse("Error updating business entity", ex.Message));
        }
    }

    /// <summary>
    /// Delete business entity (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBusinessEntity(Guid id)
    {
        try
        {
            var businessEntity = await _context.BusinessEntities.FindAsync(id);
            if (businessEntity == null)
            {
                return NotFound(ApiResponse.CreateError("Business entity not found"));
            }

            businessEntity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Business entity deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting business entity", ex.Message));
        }
    }

    private async Task<bool> BusinessEntityExists(Guid id)
    {
        return await _context.BusinessEntities.AnyAsync(e => e.Id == id);
    }
}