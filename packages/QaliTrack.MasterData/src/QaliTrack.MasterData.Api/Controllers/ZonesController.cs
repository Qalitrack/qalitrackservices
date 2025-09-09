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
[Route("zones")]
public class ZonesController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public ZonesController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Zones

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ZoneDto>>>> GetZones([FromQuery] QueryParameters queryParams)
    {
        var query = _context.Zones.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(z => z.Name.Contains(queryParams.Search) || z.Code.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var zones = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var zoneDtos = _mapper.Map<IEnumerable<ZoneDto>>(zones);

        return Ok(new ApiResponse<IEnumerable<ZoneDto>>
        {
            Data = zoneDtos,
            Success = true,
            Message = "Zones retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ZoneDto>>> GetZone(Guid id)
    {
        var zone = await _context.Zones.FindAsync(id);
        if (zone == null)
        {
            return NotFound(new ApiResponse<ZoneDto> { Success = false, Message = "Zone not found" });
        }

        var zoneDto = _mapper.Map<ZoneDto>(zone);
        return Ok(new ApiResponse<ZoneDto> { Data = zoneDto, Success = true, Message = "Zone retrieved successfully" });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ZoneDto>>> CreateZone(CreateZoneDto createZoneDto)
    {
        try
        {
            var zone = _mapper.Map<Zone>(createZoneDto);
            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            var zoneDto = _mapper.Map<ZoneDto>(zone);
            return CreatedAtAction(nameof(GetZone), new { id = zone.Id }, 
                new ApiResponse<ZoneDto> { Data = zoneDto, Success = true, Message = "Zone created successfully" });
        }
        catch (UniqueConstraintException)
        {
            return BadRequest(new ApiResponse<ZoneDto> 
            { 
                Success = false, 
                Message = "A zone with this code already exists" 
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ZoneDto>>> UpdateZone(Guid id, UpdateZoneDto updateZoneDto)
    {
        var zone = await _context.Zones.FindAsync(id);
        if (zone == null)
        {
            return NotFound(new ApiResponse<ZoneDto> { Success = false, Message = "Zone not found" });
        }

        _mapper.Map(updateZoneDto, zone);
        await _context.SaveChangesAsync();

        var zoneDto = _mapper.Map<ZoneDto>(zone);
        return Ok(new ApiResponse<ZoneDto> { Data = zoneDto, Success = true, Message = "Zone updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteZone(Guid id)
    {
        var zone = await _context.Zones.FindAsync(id);
        if (zone == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Zone not found" });
        }

        zone.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Zone deleted successfully" });
    }

    #endregion
}