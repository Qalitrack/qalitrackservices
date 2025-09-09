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
[Route("location-types")]
public class LocationTypesController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public LocationTypesController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationTypeDto>>>> GetLocationTypes([FromQuery] QueryParameters queryParams)
    {
        var query = _context.LocationTypes.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(lt => lt.Name.Contains(queryParams.Search) || lt.Code.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var locationTypes = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var locationTypeDtos = _mapper.Map<IEnumerable<LocationTypeDto>>(locationTypes);

        return Ok(new ApiResponse<IEnumerable<LocationTypeDto>>
        {
            Data = locationTypeDtos,
            Success = true,
            Message = "Location types retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LocationTypeDto>>> GetLocationType(Guid id)
    {
        var locationType = await _context.LocationTypes.FindAsync(id);
        if (locationType == null)
        {
            return NotFound(new ApiResponse<LocationTypeDto> { Success = false, Message = "Location type not found" });
        }

        var locationTypeDto = _mapper.Map<LocationTypeDto>(locationType);
        return Ok(new ApiResponse<LocationTypeDto> { Data = locationTypeDto, Success = true, Message = "Location type retrieved successfully" });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LocationTypeDto>>> CreateLocationType(CreateLocationTypeDto createLocationTypeDto)
    {
        try
        {
            var locationType = _mapper.Map<LocationType>(createLocationTypeDto);
            _context.LocationTypes.Add(locationType);
            await _context.SaveChangesAsync();

            var locationTypeDto = _mapper.Map<LocationTypeDto>(locationType);
            return CreatedAtAction(nameof(GetLocationType), new { id = locationType.Id },
                new ApiResponse<LocationTypeDto> { Data = locationTypeDto, Success = true, Message = "Location type created successfully" });
        }
        catch (UniqueConstraintException)
        {
            return BadRequest(new ApiResponse<LocationTypeDto> 
            { 
                Success = false, 
                Message = "A location type with this code already exists" 
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<LocationTypeDto>>> UpdateLocationType(Guid id, UpdateLocationTypeDto updateLocationTypeDto)
    {
        var locationType = await _context.LocationTypes.FindAsync(id);
        if (locationType == null)
        {
            return NotFound(new ApiResponse<LocationTypeDto> { Success = false, Message = "Location type not found" });
        }

        _mapper.Map(updateLocationTypeDto, locationType);
        await _context.SaveChangesAsync();

        var locationTypeDto = _mapper.Map<LocationTypeDto>(locationType);
        return Ok(new ApiResponse<LocationTypeDto> { Data = locationTypeDto, Success = true, Message = "Location type updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLocationType(Guid id)
    {
        var locationType = await _context.LocationTypes.FindAsync(id);
        if (locationType == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Location type not found" });
        }

        locationType.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Location type deleted successfully" });
    }
}