using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Infrastructure.Data;
using QaliTrack.MasterData.Api.Profiles;
using AutoMapper;
using EntityFramework.Exceptions.Common;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("drivers")]
public class DriverController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public DriverController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DriverDto>>>> GetDrivers([FromQuery] QueryParameters queryParams)
    {
        var query = _context.Drivers.Where(d => !d.IsDeleted).AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(d => d.FirstName.Contains(queryParams.Search) || 
                                   d.LastName.Contains(queryParams.Search) ||
                                   d.PhoneNumber.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var drivers = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var driverDtos = _mapper.Map<IEnumerable<DriverDto>>(drivers);

        return Ok(new ApiResponse<IEnumerable<DriverDto>>
        {
            Data = driverDtos,
            Success = true,
            Message = "Drivers retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DriverDto>>> GetDriver(Guid id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null || driver.IsDeleted)
        {
            return NotFound(new ApiResponse<DriverDto> { Success = false, Message = "Driver not found" });
        }

        var driverDto = _mapper.Map<DriverDto>(driver);
        return Ok(new ApiResponse<DriverDto> { Data = driverDto, Success = true, Message = "Driver retrieved successfully" });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DriverDto>>> CreateDriver(CreateDriverDto createDriverDto)
    {
        try
        {
            var driver = _mapper.Map<Driver>(createDriverDto);
            driver.Id = Guid.NewGuid();
            driver.CreatedAt = DateTime.UtcNow;
            driver.UpdatedAt = DateTime.UtcNow;
            
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            var driverDto = _mapper.Map<DriverDto>(driver);
            return CreatedAtAction(nameof(GetDriver), new { id = driver.Id },
                new ApiResponse<DriverDto> { Data = driverDto, Success = true, Message = "Driver created successfully" });
        }
        catch (UniqueConstraintException)
        {
            return BadRequest(new ApiResponse<DriverDto> 
            { 
                Success = false, 
                Message = "A driver with this employee ID or license number already exists" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<DriverDto>
            {
                Success = false,
                Message = "Error creating driver",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DriverDto>>> UpdateDriver(Guid id, UpdateDriverDto updateDriverDto)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null || driver.IsDeleted)
        {
            return NotFound(new ApiResponse<DriverDto> { Success = false, Message = "Driver not found" });
        }

        _mapper.Map(updateDriverDto, driver);
        driver.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var driverDto = _mapper.Map<DriverDto>(driver);
        return Ok(new ApiResponse<DriverDto> { Data = driverDto, Success = true, Message = "Driver updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriver(Guid id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null || driver.IsDeleted)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Driver not found" });
        }

        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Driver deleted successfully" });
    }
}