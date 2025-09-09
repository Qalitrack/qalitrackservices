using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Core.Common;
using QaliTrack.DataManager.Core.Modules.WeightData.Entities;
using QaliTrack.DataManager.Infrastructure.Data;

namespace QaliTrack.DataManager.Api.Controllers;

[ApiController]
[Route("weight-data")]
public class WeightDataController : ControllerBase
{
    private readonly DataManagerDbContext _context;

    public WeightDataController(DataManagerDbContext context)
    {
        _context = context;
    }

    [HttpGet("measurements")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeightMeasurement>>>> GetMeasurements([FromQuery] QueryParameters queryParams)
    {
        var query = _context.WeightMeasurements.AsQueryable();

        var totalCount = await query.CountAsync();
        var measurements = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<WeightMeasurement>>
        {
            Data = measurements,
            Success = true,
            Message = "Measurements retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("calibrations")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalibrationRecord>>>> GetCalibrations([FromQuery] QueryParameters queryParams)
    {
        var query = _context.CalibrationRecords.AsQueryable();

        var totalCount = await query.CountAsync();
        var calibrations = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<CalibrationRecord>>
        {
            Data = calibrations,
            Success = true,
            Message = "Calibrations retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }
}