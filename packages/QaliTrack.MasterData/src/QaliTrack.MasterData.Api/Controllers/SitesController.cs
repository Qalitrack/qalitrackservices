using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.SiteManagement.DTOs;
using QaliTrack.MasterData.Core.Modules.SiteManagement.Entities;
using QaliTrack.MasterData.Infrastructure.Data;
using AutoMapper;
using EntityFramework.Exceptions.Common;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("sites")]
public class SitesController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public SitesController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SiteDto>>>> GetSites([FromQuery] QueryParameters queryParams)
    {
        var query = _context.Sites
            .Include(s => s.LocationType)
            .Include(s => s.Zone)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(s => s.Name.Contains(queryParams.Search) || 
                                   s.City.Contains(queryParams.Search) || 
                                   s.ContactPerson.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var sites = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var siteDtos = _mapper.Map<IEnumerable<SiteDto>>(sites);

        return Ok(new ApiResponse<IEnumerable<SiteDto>>
        {
            Data = siteDtos,
            Success = true,
            Message = "Sites retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SiteDto>>> GetSite(Guid id)
    {
        var site = await _context.Sites
            .Include(s => s.LocationType)
            .Include(s => s.Zone)
            .FirstOrDefaultAsync(s => s.Id == id);
            
        if (site == null)
        {
            return NotFound(new ApiResponse<SiteDto> { Success = false, Message = "Site not found" });
        }

        var siteDto = _mapper.Map<SiteDto>(site);
        return Ok(new ApiResponse<SiteDto> { Data = siteDto, Success = true, Message = "Site retrieved successfully" });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SiteDto>>> CreateSite(CreateSiteDto createSiteDto)
    {
        try
        {
            var site = _mapper.Map<Site>(createSiteDto);
            _context.Sites.Add(site);
            await _context.SaveChangesAsync();

            var siteDto = _mapper.Map<SiteDto>(site);
            return CreatedAtAction(nameof(GetSite), new { id = site.Id },
                new ApiResponse<SiteDto> { Data = siteDto, Success = true, Message = "Site created successfully" });
        }
        catch (UniqueConstraintException)
        {
            return BadRequest(new ApiResponse<SiteDto> 
            { 
                Success = false, 
                Message = "A site with this name already exists" 
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SiteDto>>> UpdateSite(Guid id, UpdateSiteDto updateSiteDto)
    {
        var site = await _context.Sites.FindAsync(id);
        if (site == null)
        {
            return NotFound(new ApiResponse<SiteDto> { Success = false, Message = "Site not found" });
        }

        _mapper.Map(updateSiteDto, site);
        await _context.SaveChangesAsync();

        var siteDto = _mapper.Map<SiteDto>(site);
        return Ok(new ApiResponse<SiteDto> { Data = siteDto, Success = true, Message = "Site updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSite(Guid id)
    {
        var site = await _context.Sites.FindAsync(id);
        if (site == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Site not found" });
        }

        site.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Site deleted successfully" });
    }
}