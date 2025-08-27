using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Organization.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Organization Module")]
public class OrganizationController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public OrganizationController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get organizations with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of organizations</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Organization>>>> GetOrganizations(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Organizations
                .Include(o => o.Settings)
                .Include(o => o.Subscription)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<Organization>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Organization>>.ErrorResponse("Error retrieving organizations", ex.Message));
        }
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Organization>>> GetOrganization(Guid id)
    {
        try
        {
            var organization = await _context.Organizations
                .Include(o => o.Users)
                .Include(o => o.Locations)
                .Include(o => o.Settings)
                .Include(o => o.Subscription)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
            {
                return NotFound(ApiResponse<Organization>.ErrorResponse("Organization not found"));
            }

            return Ok(ApiResponse<Organization>.SuccessResponse(organization));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Organization>.ErrorResponse("Error retrieving organization", ex.Message));
        }
    }

    /// <summary>
    /// Create new organization
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Organization>>> CreateOrganization(Organization organization)
    {
        try
        {
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganization), 
                new { id = organization.Id }, 
                ApiResponse<Organization>.SuccessResponse(organization, "Organization created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Organization>.ErrorResponse("Error creating organization", ex.Message));
        }
    }

    /// <summary>
    /// Update organization
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Organization>>> UpdateOrganization(Guid id, Organization organization)
    {
        if (id != organization.Id)
        {
            return BadRequest(ApiResponse<Organization>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(organization).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Organization>.SuccessResponse(organization, "Organization updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await OrganizationExists(id))
            {
                return NotFound(ApiResponse<Organization>.ErrorResponse("Organization not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Organization>.ErrorResponse("Error updating organization", ex.Message));
        }
    }

    /// <summary>
    /// Delete organization (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOrganization(Guid id)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound(ApiResponse.CreateError("Organization not found"));
            }

            organization.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Organization deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting organization", ex.Message));
        }
    }

    private async Task<bool> OrganizationExists(Guid id)
    {
        return await _context.Organizations.AnyAsync(e => e.Id == id);
    }
}