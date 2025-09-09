using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Infrastructure.Data;
using QaliTrack.MasterData.Api.Profiles;
using AutoMapper;
using EntityFramework.Exceptions.Common;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("vehicles")]
public class VehicleController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public VehicleController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleDto>>>> GetVehicles([FromQuery] QueryParameters queryParams)
    {
        var query = _context.Vehicles.Where(v => !v.IsDeleted).AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(v => v.NumberPlate.Contains(queryParams.Search) || 
                                   v.Make.Contains(queryParams.Search) ||
                                   v.Model.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var vehicles = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var vehicleDtos = _mapper.Map<IEnumerable<VehicleDto>>(vehicles);

        return Ok(new ApiResponse<IEnumerable<VehicleDto>>
        {
            Data = vehicleDtos,
            Success = true,
            Message = "Vehicles retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicle(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return NotFound(new ApiResponse<VehicleDto> { Success = false, Message = "Vehicle not found" });
        }

        var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
        return Ok(new ApiResponse<VehicleDto> { Data = vehicleDto, Success = true, Message = "Vehicle retrieved successfully" });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> CreateVehicle(CreateVehicleDto createVehicleDto)
    {
        try
        {
            var vehicle = _mapper.Map<Vehicle>(createVehicleDto);
            vehicle.Id = Guid.NewGuid();
            vehicle.CreatedAt = DateTime.UtcNow;
            vehicle.UpdatedAt = DateTime.UtcNow;
            
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id },
                new ApiResponse<VehicleDto> { Data = vehicleDto, Success = true, Message = "Vehicle created successfully" });
        }
        catch (UniqueConstraintException)
        {
            return BadRequest(new ApiResponse<VehicleDto> 
            { 
                Success = false, 
                Message = "A vehicle with this registration number already exists" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<VehicleDto>
            {
                Success = false,
                Message = "Error creating vehicle",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> UpdateVehicle(Guid id, UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return NotFound(new ApiResponse<VehicleDto> { Success = false, Message = "Vehicle not found" });
        }

        _mapper.Map(updateVehicleDto, vehicle);
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
        return Ok(new ApiResponse<VehicleDto> { Data = vehicleDto, Success = true, Message = "Vehicle updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicle(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null || vehicle.IsDeleted)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Vehicle not found" });
        }

        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Vehicle deleted successfully" });
    }
}