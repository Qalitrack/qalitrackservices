using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Core.Common;
using QaliTrack.DataManager.Core.Modules.Operations.DTOs;
using QaliTrack.DataManager.Core.Modules.Operations.Entities;
using QaliTrack.DataManager.Infrastructure.Data;
using AutoMapper;

namespace QaliTrack.DataManager.Api.Controllers;

[ApiController]
[Route("operations")]
[Tags("Operations Module")]
public class OperationsController : ControllerBase
{
    private readonly DataManagerDbContext _context;
    private readonly IMapper _mapper;

    public OperationsController(DataManagerDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get operational alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OperationalAlertDto>>>> GetOperationalAlerts(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.OperationalAlerts.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(oa => oa.AlertType.Contains(queryParams.Search) || 
                                     oa.Message.Contains(queryParams.Search) ||
                                     oa.EntityType.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var alerts = await query
            .OrderByDescending(oa => oa.AlertTime)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var alertDtos = _mapper.Map<IEnumerable<OperationalAlertDto>>(alerts);

        return Ok(new ApiResponse<IEnumerable<OperationalAlertDto>>
        {
            Data = alertDtos,
            Success = true,
            Message = "Operational alerts retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get operational alert by ID
    /// </summary>
    [HttpGet("alerts/{id}")]
    public async Task<ActionResult<ApiResponse<OperationalAlertDto>>> GetOperationalAlert(Guid id)
    {
        var alert = await _context.OperationalAlerts.FindAsync(id);
        if (alert == null)
        {
            return NotFound(new ApiResponse<OperationalAlertDto> { Success = false, Message = "Operational alert not found" });
        }

        var alertDto = _mapper.Map<OperationalAlertDto>(alert);
        return Ok(new ApiResponse<OperationalAlertDto> { Data = alertDto, Success = true, Message = "Operational alert retrieved successfully" });
    }

    /// <summary>
    /// Get active operational alerts only
    /// </summary>
    [HttpGet("alerts/active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OperationalAlertDto>>>> GetActiveOperationalAlerts(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.OperationalAlerts
            .Where(oa => oa.Status == "Open" || oa.Status == "InProgress")
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(oa => oa.AlertType.Contains(queryParams.Search) || 
                                     oa.Message.Contains(queryParams.Search) ||
                                     oa.EntityType.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var alerts = await query
            .OrderByDescending(oa => oa.AlertTime)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var alertDtos = _mapper.Map<IEnumerable<OperationalAlertDto>>(alerts);

        return Ok(new ApiResponse<IEnumerable<OperationalAlertDto>>
        {
            Data = alertDtos,
            Success = true,
            Message = "Active operational alerts retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Create new operational alert
    /// </summary>
    [HttpPost("alerts")]
    public async Task<ActionResult<ApiResponse<OperationalAlertDto>>> CreateOperationalAlert(CreateOperationalAlertDto createAlertDto)
    {
        var alert = _mapper.Map<OperationalAlert>(createAlertDto);
        _context.OperationalAlerts.Add(alert);
        await _context.SaveChangesAsync();

        var alertDto = _mapper.Map<OperationalAlertDto>(alert);
        return CreatedAtAction(nameof(GetOperationalAlert), new { id = alert.Id },
            new ApiResponse<OperationalAlertDto> { Data = alertDto, Success = true, Message = "Operational alert created successfully" });
    }

    /// <summary>
    /// Update operational alert
    /// </summary>
    [HttpPut("alerts/{id}")]
    public async Task<ActionResult<ApiResponse<OperationalAlertDto>>> UpdateOperationalAlert(Guid id, UpdateOperationalAlertDto updateAlertDto)
    {
        var alert = await _context.OperationalAlerts.FindAsync(id);
        if (alert == null)
        {
            return NotFound(new ApiResponse<OperationalAlertDto> { Success = false, Message = "Operational alert not found" });
        }

        _mapper.Map(updateAlertDto, alert);
        await _context.SaveChangesAsync();

        var alertDto = _mapper.Map<OperationalAlertDto>(alert);
        return Ok(new ApiResponse<OperationalAlertDto> { Data = alertDto, Success = true, Message = "Operational alert updated successfully" });
    }

    /// <summary>
    /// Delete operational alert
    /// </summary>
    [HttpDelete("alerts/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOperationalAlert(Guid id)
    {
        var alert = await _context.OperationalAlerts.FindAsync(id);
        if (alert == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Operational alert not found" });
        }

        _context.OperationalAlerts.Remove(alert);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Operational alert deleted successfully" });
    }
}