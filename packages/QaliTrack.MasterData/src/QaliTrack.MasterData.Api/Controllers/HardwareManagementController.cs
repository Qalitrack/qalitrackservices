using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.HardwareManagement.DTOs;
using QaliTrack.MasterData.Core.Modules.HardwareManagement.Entities;
using QaliTrack.MasterData.Infrastructure.Data;
using AutoMapper;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("hardwaremanagement")]
[Tags("Hardware Management Module")]
public class HardwareManagementController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public HardwareManagementController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Weighbridges

    /// <summary>
    /// Get weighbridges
    /// </summary>
    [HttpGet("weighbridges")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeighbridgeDto>>>> GetWeighbridges(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.Weighbridges.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(w => w.Name.Contains(queryParams.Search) || w.Code.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var weighbridges = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var weighbridgeDtos = _mapper.Map<IEnumerable<WeighbridgeDto>>(weighbridges);

        return Ok(new ApiResponse<IEnumerable<WeighbridgeDto>>
        {
            Data = weighbridgeDtos,
            Success = true,
            Message = "Weighbridges retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get weighbridge by ID
    /// </summary>
    [HttpGet("weighbridges/{id}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDto>>> GetWeighbridge(Guid id)
    {
        var weighbridge = await _context.Weighbridges.FindAsync(id);

        if (weighbridge == null)
        {
            return NotFound(new ApiResponse<WeighbridgeDto> { Success = false, Message = "Weighbridge not found" });
        }

        var weighbridgeDto = _mapper.Map<WeighbridgeDto>(weighbridge);
        return Ok(new ApiResponse<WeighbridgeDto> { Data = weighbridgeDto, Success = true, Message = "Weighbridge retrieved successfully" });
    }

    /// <summary>
    /// Get weighbridges by site ID
    /// </summary>
    [HttpGet("weighbridges/by-site/{siteId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeighbridgeDto>>>> GetWeighbridgesBySite(Guid siteId)
    {
        var weighbridges = await _context.Weighbridges
            .Where(w => w.SiteId == siteId)
            .ToListAsync();

        var weighbridgeDtos = _mapper.Map<IEnumerable<WeighbridgeDto>>(weighbridges);
        return Ok(new ApiResponse<IEnumerable<WeighbridgeDto>> 
        { 
            Data = weighbridgeDtos, 
            Success = true, 
            Message = "Weighbridges retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new weighbridge
    /// </summary>
    [HttpPost("weighbridges")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDto>>> CreateWeighbridge(CreateWeighbridgeDto createWeighbridgeDto)
    {
        var weighbridge = _mapper.Map<Weighbridge>(createWeighbridgeDto);
        _context.Weighbridges.Add(weighbridge);
        await _context.SaveChangesAsync();

        var weighbridgeDto = _mapper.Map<WeighbridgeDto>(weighbridge);
        return CreatedAtAction(nameof(GetWeighbridge), new { id = weighbridge.Id },
            new ApiResponse<WeighbridgeDto> { Data = weighbridgeDto, Success = true, Message = "Weighbridge created successfully" });
    }

    /// <summary>
    /// Update weighbridge
    /// </summary>
    [HttpPut("weighbridges/{id}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDto>>> UpdateWeighbridge(Guid id, UpdateWeighbridgeDto updateWeighbridgeDto)
    {
        var weighbridge = await _context.Weighbridges.FindAsync(id);
        if (weighbridge == null)
        {
            return NotFound(new ApiResponse<WeighbridgeDto> { Success = false, Message = "Weighbridge not found" });
        }

        _mapper.Map(updateWeighbridgeDto, weighbridge);
        await _context.SaveChangesAsync();

        var weighbridgeDto = _mapper.Map<WeighbridgeDto>(weighbridge);
        return Ok(new ApiResponse<WeighbridgeDto> { Data = weighbridgeDto, Success = true, Message = "Weighbridge updated successfully" });
    }

    /// <summary>
    /// Delete weighbridge
    /// </summary>
    [HttpDelete("weighbridges/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridge(Guid id)
    {
        var weighbridge = await _context.Weighbridges.FindAsync(id);
        if (weighbridge == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Weighbridge not found" });
        }

        _context.Weighbridges.Remove(weighbridge);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Weighbridge deleted successfully" });
    }

    #endregion

    #region PLC Configurations

    /// <summary>
    /// Get PLC configurations
    /// </summary>
    [HttpGet("plc-configurations")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlcConfigurationDto>>>> GetPlcConfigurations(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.PlcConfigurations.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(pc => pc.PlcType.Contains(queryParams.Search) || pc.PlcAddress.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var plcConfigurations = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var plcConfigurationDtos = _mapper.Map<IEnumerable<PlcConfigurationDto>>(plcConfigurations);

        return Ok(new ApiResponse<IEnumerable<PlcConfigurationDto>>
        {
            Data = plcConfigurationDtos,
            Success = true,
            Message = "PLC configurations retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get PLC configuration by ID
    /// </summary>
    [HttpGet("plc-configurations/{id}")]
    public async Task<ActionResult<ApiResponse<PlcConfigurationDto>>> GetPlcConfiguration(Guid id)
    {
        var plcConfiguration = await _context.PlcConfigurations.FindAsync(id);

        if (plcConfiguration == null)
        {
            return NotFound(new ApiResponse<PlcConfigurationDto> { Success = false, Message = "PLC configuration not found" });
        }

        var plcConfigurationDto = _mapper.Map<PlcConfigurationDto>(plcConfiguration);
        return Ok(new ApiResponse<PlcConfigurationDto> { Data = plcConfigurationDto, Success = true, Message = "PLC configuration retrieved successfully" });
    }

    /// <summary>
    /// Get PLC configurations by weighbridge ID
    /// </summary>
    [HttpGet("plc-configurations/by-weighbridge/{weighbridgeId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlcConfigurationDto>>>> GetPlcConfigurationsByWeighbridge(Guid weighbridgeId)
    {
        var plcConfigurations = await _context.PlcConfigurations
            .Where(pc => pc.WeighbridgeId == weighbridgeId)
            .ToListAsync();

        var plcConfigurationDtos = _mapper.Map<IEnumerable<PlcConfigurationDto>>(plcConfigurations);
        return Ok(new ApiResponse<IEnumerable<PlcConfigurationDto>> 
        { 
            Data = plcConfigurationDtos, 
            Success = true, 
            Message = "PLC configurations retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new PLC configuration
    /// </summary>
    [HttpPost("plc-configurations")]
    public async Task<ActionResult<ApiResponse<PlcConfigurationDto>>> CreatePlcConfiguration(CreatePlcConfigurationDto createPlcConfigurationDto)
    {
        var plcConfiguration = _mapper.Map<PlcConfiguration>(createPlcConfigurationDto);
        _context.PlcConfigurations.Add(plcConfiguration);
        await _context.SaveChangesAsync();

        var plcConfigurationDto = _mapper.Map<PlcConfigurationDto>(plcConfiguration);
        return CreatedAtAction(nameof(GetPlcConfiguration), new { id = plcConfiguration.Id },
            new ApiResponse<PlcConfigurationDto> { Data = plcConfigurationDto, Success = true, Message = "PLC configuration created successfully" });
    }

    /// <summary>
    /// Update PLC configuration
    /// </summary>
    [HttpPut("plc-configurations/{id}")]
    public async Task<ActionResult<ApiResponse<PlcConfigurationDto>>> UpdatePlcConfiguration(Guid id, UpdatePlcConfigurationDto updatePlcConfigurationDto)
    {
        var plcConfiguration = await _context.PlcConfigurations.FindAsync(id);
        if (plcConfiguration == null)
        {
            return NotFound(new ApiResponse<PlcConfigurationDto> { Success = false, Message = "PLC configuration not found" });
        }

        _mapper.Map(updatePlcConfigurationDto, plcConfiguration);
        await _context.SaveChangesAsync();

        var plcConfigurationDto = _mapper.Map<PlcConfigurationDto>(plcConfiguration);
        return Ok(new ApiResponse<PlcConfigurationDto> { Data = plcConfigurationDto, Success = true, Message = "PLC configuration updated successfully" });
    }

    /// <summary>
    /// Delete PLC configuration
    /// </summary>
    [HttpDelete("plc-configurations/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePlcConfiguration(Guid id)
    {
        var plcConfiguration = await _context.PlcConfigurations.FindAsync(id);
        if (plcConfiguration == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "PLC configuration not found" });
        }

        _context.PlcConfigurations.Remove(plcConfiguration);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "PLC configuration deleted successfully" });
    }

    #endregion

    #region ANPR Cameras

    /// <summary>
    /// Get ANPR cameras
    /// </summary>
    [HttpGet("anpr-cameras")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AnprCameraDto>>>> GetAnprCameras(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.AnprCameras.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(ac => ac.CameraName.Contains(queryParams.Search) || ac.Model.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var anprCameras = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var anprCameraDtos = _mapper.Map<IEnumerable<AnprCameraDto>>(anprCameras);

        return Ok(new ApiResponse<IEnumerable<AnprCameraDto>>
        {
            Data = anprCameraDtos,
            Success = true,
            Message = "ANPR cameras retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get ANPR camera by ID
    /// </summary>
    [HttpGet("anpr-cameras/{id}")]
    public async Task<ActionResult<ApiResponse<AnprCameraDto>>> GetAnprCamera(Guid id)
    {
        var anprCamera = await _context.AnprCameras.FindAsync(id);

        if (anprCamera == null)
        {
            return NotFound(new ApiResponse<AnprCameraDto> { Success = false, Message = "ANPR camera not found" });
        }

        var anprCameraDto = _mapper.Map<AnprCameraDto>(anprCamera);
        return Ok(new ApiResponse<AnprCameraDto> { Data = anprCameraDto, Success = true, Message = "ANPR camera retrieved successfully" });
    }

    /// <summary>
    /// Get ANPR cameras by weighbridge ID
    /// </summary>
    [HttpGet("anpr-cameras/by-weighbridge/{weighbridgeId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AnprCameraDto>>>> GetAnprCamerasByWeighbridge(Guid weighbridgeId)
    {
        var anprCameras = await _context.AnprCameras
            .Where(ac => ac.WeighbridgeId == weighbridgeId)
            .ToListAsync();

        var anprCameraDtos = _mapper.Map<IEnumerable<AnprCameraDto>>(anprCameras);
        return Ok(new ApiResponse<IEnumerable<AnprCameraDto>> 
        { 
            Data = anprCameraDtos, 
            Success = true, 
            Message = "ANPR cameras retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new ANPR camera
    /// </summary>
    [HttpPost("anpr-cameras")]
    public async Task<ActionResult<ApiResponse<AnprCameraDto>>> CreateAnprCamera(CreateAnprCameraDto createAnprCameraDto)
    {
        var anprCamera = _mapper.Map<AnprCamera>(createAnprCameraDto);
        _context.AnprCameras.Add(anprCamera);
        await _context.SaveChangesAsync();

        var anprCameraDto = _mapper.Map<AnprCameraDto>(anprCamera);
        return CreatedAtAction(nameof(GetAnprCamera), new { id = anprCamera.Id },
            new ApiResponse<AnprCameraDto> { Data = anprCameraDto, Success = true, Message = "ANPR camera created successfully" });
    }

    /// <summary>
    /// Update ANPR camera
    /// </summary>
    [HttpPut("anpr-cameras/{id}")]
    public async Task<ActionResult<ApiResponse<AnprCameraDto>>> UpdateAnprCamera(Guid id, UpdateAnprCameraDto updateAnprCameraDto)
    {
        var anprCamera = await _context.AnprCameras.FindAsync(id);
        if (anprCamera == null)
        {
            return NotFound(new ApiResponse<AnprCameraDto> { Success = false, Message = "ANPR camera not found" });
        }

        _mapper.Map(updateAnprCameraDto, anprCamera);
        await _context.SaveChangesAsync();

        var anprCameraDto = _mapper.Map<AnprCameraDto>(anprCamera);
        return Ok(new ApiResponse<AnprCameraDto> { Data = anprCameraDto, Success = true, Message = "ANPR camera updated successfully" });
    }

    /// <summary>
    /// Delete ANPR camera
    /// </summary>
    [HttpDelete("anpr-cameras/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAnprCamera(Guid id)
    {
        var anprCamera = await _context.AnprCameras.FindAsync(id);
        if (anprCamera == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "ANPR camera not found" });
        }

        _context.AnprCameras.Remove(anprCamera);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "ANPR camera deleted successfully" });
    }

    #endregion
}