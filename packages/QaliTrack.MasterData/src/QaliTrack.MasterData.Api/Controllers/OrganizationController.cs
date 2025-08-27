using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Organization.Entities;
using QaliTrack.MasterData.Core.Modules.Organization.DTOs;
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
    public async Task<ActionResult<ApiResponse<PagedResult<OrganizationSummaryDto>>>> GetOrganizations(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Organizations
                .Select(o => new OrganizationSummaryDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Code = o.Code,
                    Type = o.Type,
                    Status = o.Status,
                    City = o.City,
                    State = o.State,
                    Country = o.Country,
                    ContactEmail = o.ContactEmail,
                    EstablishedDate = o.EstablishedDate,
                    CreatedAt = o.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<OrganizationSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<OrganizationSummaryDto>>.ErrorResponse("Error retrieving organizations", ex.Message));
        }
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDetailDto>>> GetOrganization(Guid id)
    {
        try
        {
            var organization = await _context.Organizations
                .Include(o => o.Users)
                .Include(o => o.Locations)
                .Include(o => o.Settings)
                .Include(o => o.Subscription)
                .Where(o => o.Id == id)
                .Select(o => new OrganizationDetailDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Code = o.Code,
                    Description = o.Description,
                    Type = o.Type,
                    Status = o.Status,
                    Address = o.Address,
                    City = o.City,
                    State = o.State,
                    Country = o.Country,
                    PostalCode = o.PostalCode,
                    ContactEmail = o.ContactEmail,
                    ContactPhone = o.ContactPhone,
                    Website = o.Website,
                    TaxNumber = o.TaxNumber,
                    RegistrationNumber = o.RegistrationNumber,
                    EstablishedDate = o.EstablishedDate,
                    LogoUrl = o.LogoUrl,
                    Notes = o.Notes,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    HasSettings = o.Settings != null,
                    HasSubscription = o.Subscription != null,
                    HasActiveUsers = o.Users.Any(u => u.Status == "Active"),
                    HasLocations = o.Locations.Any()
                })
                .FirstOrDefaultAsync();

            if (organization == null)
            {
                return NotFound(ApiResponse<OrganizationDetailDto>.ErrorResponse("Organization not found"));
            }

            return Ok(ApiResponse<OrganizationDetailDto>.SuccessResponse(organization));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<OrganizationDetailDto>.ErrorResponse("Error retrieving organization", ex.Message));
        }
    }

    /// <summary>
    /// Create new organization
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrganizationDetailDto>>> CreateOrganization(CreateOrganizationDto dto)
    {
        try
        {
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Type = dto.Type,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode ?? string.Empty,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Website = dto.Website,
                TaxNumber = dto.TaxNumber,
                RegistrationNumber = dto.RegistrationNumber,
                EstablishedDate = dto.EstablishedDate,
                LogoUrl = dto.LogoUrl,
                Notes = dto.Notes,
                Status = "Active"
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            // Return detailed response
            var responseDto = new OrganizationDetailDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Code = organization.Code,
                Description = organization.Description,
                Type = organization.Type,
                Status = organization.Status,
                Address = organization.Address,
                City = organization.City,
                State = organization.State,
                Country = organization.Country,
                PostalCode = organization.PostalCode,
                ContactEmail = organization.ContactEmail,
                ContactPhone = organization.ContactPhone,
                Website = organization.Website,
                TaxNumber = organization.TaxNumber,
                RegistrationNumber = organization.RegistrationNumber,
                EstablishedDate = organization.EstablishedDate,
                LogoUrl = organization.LogoUrl,
                Notes = organization.Notes,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt,
                HasSettings = false,
                HasSubscription = false,
                HasActiveUsers = false,
                HasLocations = false
            };

            return CreatedAtAction(nameof(GetOrganization), 
                new { id = organization.Id }, 
                ApiResponse<OrganizationDetailDto>.SuccessResponse(responseDto, "Organization created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<OrganizationDetailDto>.ErrorResponse("Error creating organization", ex.Message));
        }
    }

    /// <summary>
    /// Update organization
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDetailDto>>> UpdateOrganization(Guid id, UpdateOrganizationDto dto)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound(ApiResponse<OrganizationDetailDto>.ErrorResponse("Organization not found"));
            }

            // Update properties from DTO
            organization.Name = dto.Name;
            organization.Description = dto.Description;
            organization.Status = dto.Status;
            organization.Address = dto.Address;
            organization.City = dto.City;
            organization.State = dto.State;
            organization.Country = dto.Country;
            organization.PostalCode = dto.PostalCode ?? organization.PostalCode;
            organization.ContactEmail = dto.ContactEmail;
            organization.ContactPhone = dto.ContactPhone;
            organization.Website = dto.Website;
            organization.LogoUrl = dto.LogoUrl;
            organization.Notes = dto.Notes;
            organization.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated organization details
            var responseDto = await _context.Organizations
                .Include(o => o.Users)
                .Include(o => o.Locations)
                .Include(o => o.Settings)
                .Include(o => o.Subscription)
                .Where(o => o.Id == id)
                .Select(o => new OrganizationDetailDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Code = o.Code,
                    Description = o.Description,
                    Type = o.Type,
                    Status = o.Status,
                    Address = o.Address,
                    City = o.City,
                    State = o.State,
                    Country = o.Country,
                    PostalCode = o.PostalCode,
                    ContactEmail = o.ContactEmail,
                    ContactPhone = o.ContactPhone,
                    Website = o.Website,
                    TaxNumber = o.TaxNumber,
                    RegistrationNumber = o.RegistrationNumber,
                    EstablishedDate = o.EstablishedDate,
                    LogoUrl = o.LogoUrl,
                    Notes = o.Notes,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    HasSettings = o.Settings != null,
                    HasSubscription = o.Subscription != null,
                    HasActiveUsers = o.Users.Any(u => u.Status == "Active"),
                    HasLocations = o.Locations.Any()
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<OrganizationDetailDto>.SuccessResponse(responseDto!, "Organization updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<OrganizationDetailDto>.ErrorResponse("Error updating organization", ex.Message));
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

    /// <summary>
    /// Get organization users
    /// </summary>
    [HttpGet("{id}/users")]
    public async Task<ActionResult<ApiResponse<PagedResult<OrganizationUserDto>>>> GetOrganizationUsers(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.OrganizationUsers
                .Where(u => u.OrganizationId == id)
                .Select(u => new OrganizationUserDto
                {
                    Id = u.Id,
                    OrganizationId = u.OrganizationId,
                    UserId = u.UserId,
                    Role = u.Role,
                    Status = u.Status,
                    JoinedDate = u.JoinedDate,
                    LeftDate = u.LeftDate,
                    Notes = u.Notes,
                    CreatedAt = u.CreatedAt
                })
                .OrderByDescending(u => u.JoinedDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<OrganizationUserDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<OrganizationUserDto>>.ErrorResponse("Error retrieving organization users", ex.Message));
        }
    }

    /// <summary>
    /// Get organization locations
    /// </summary>
    [HttpGet("{id}/locations")]
    public async Task<ActionResult<ApiResponse<PagedResult<OrganizationLocationDto>>>> GetOrganizationLocations(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.OrganizationLocations
                .Where(l => l.OrganizationId == id)
                .Select(l => new OrganizationLocationDto
                {
                    Id = l.Id,
                    OrganizationId = l.OrganizationId,
                    Name = l.Name,
                    Address = l.Address,
                    City = l.City,
                    State = l.State,
                    Country = l.Country,
                    PostalCode = l.PostalCode,
                    Phone = l.ContactPhone,
                    Email = l.ContactEmail,
                    IsHeadquarters = l.IsHeadquarters,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt
                })
                .OrderBy(l => l.Name)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<OrganizationLocationDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<OrganizationLocationDto>>.ErrorResponse("Error retrieving organization locations", ex.Message));
        }
    }

    private async Task<bool> OrganizationExists(Guid id)
    {
        return await _context.Organizations.AnyAsync(e => e.Id == id);
    }
}