using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Core.Common;
using QaliTrack.DataManager.Core.Modules.Quality.DTOs;
using QaliTrack.DataManager.Core.Modules.Quality.Entities;
using QaliTrack.DataManager.Infrastructure.Data;
using AutoMapper;

namespace QaliTrack.DataManager.Api.Controllers;

[ApiController]
[Route("quality")]
[Tags("Quality Module")]
public class QualityController : ControllerBase
{
    private readonly DataManagerDbContext _context;
    private readonly IMapper _mapper;

    public QualityController(DataManagerDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Quality Test Results

    /// <summary>
    /// Get quality test results
    /// </summary>
    [HttpGet("test-results")]
    public async Task<ActionResult<ApiResponse<IEnumerable<QualityTestResultDto>>>> GetQualityTestResults(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.QualityTestResults.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(qtr => qtr.TestMethod.Contains(queryParams.Search) || 
                                      qtr.TestedBy.Contains(queryParams.Search) ||
                                      qtr.BatchNumber.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var testResults = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var testResultDtos = _mapper.Map<IEnumerable<QualityTestResultDto>>(testResults);

        return Ok(new ApiResponse<IEnumerable<QualityTestResultDto>>
        {
            Data = testResultDtos,
            Success = true,
            Message = "Quality test results retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get quality test result by ID
    /// </summary>
    [HttpGet("test-results/{id}")]
    public async Task<ActionResult<ApiResponse<QualityTestResultDto>>> GetQualityTestResult(Guid id)
    {
        var testResult = await _context.QualityTestResults.FindAsync(id);
        if (testResult == null)
        {
            return NotFound(new ApiResponse<QualityTestResultDto> { Success = false, Message = "Quality test result not found" });
        }

        var testResultDto = _mapper.Map<QualityTestResultDto>(testResult);
        return Ok(new ApiResponse<QualityTestResultDto> { Data = testResultDto, Success = true, Message = "Quality test result retrieved successfully" });
    }

    /// <summary>
    /// Get quality test results by transaction ID
    /// </summary>
    [HttpGet("test-results/by-transaction/{transactionId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<QualityTestResultDto>>>> GetQualityTestResultsByTransaction(Guid transactionId)
    {
        var testResults = await _context.QualityTestResults
            .Where(qtr => qtr.TransactionId == transactionId)
            .ToListAsync();

        var testResultDtos = _mapper.Map<IEnumerable<QualityTestResultDto>>(testResults);
        return Ok(new ApiResponse<IEnumerable<QualityTestResultDto>> 
        { 
            Data = testResultDtos, 
            Success = true, 
            Message = "Quality test results retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new quality test result
    /// </summary>
    [HttpPost("test-results")]
    public async Task<ActionResult<ApiResponse<QualityTestResultDto>>> CreateQualityTestResult(CreateQualityTestResultDto createTestResultDto)
    {
        var testResult = _mapper.Map<QualityTestResult>(createTestResultDto);
        _context.QualityTestResults.Add(testResult);
        await _context.SaveChangesAsync();

        var testResultDto = _mapper.Map<QualityTestResultDto>(testResult);
        return CreatedAtAction(nameof(GetQualityTestResult), new { id = testResult.Id },
            new ApiResponse<QualityTestResultDto> { Data = testResultDto, Success = true, Message = "Quality test result created successfully" });
    }

    /// <summary>
    /// Update quality test result
    /// </summary>
    [HttpPut("test-results/{id}")]
    public async Task<ActionResult<ApiResponse<QualityTestResultDto>>> UpdateQualityTestResult(Guid id, UpdateQualityTestResultDto updateTestResultDto)
    {
        var testResult = await _context.QualityTestResults.FindAsync(id);
        if (testResult == null)
        {
            return NotFound(new ApiResponse<QualityTestResultDto> { Success = false, Message = "Quality test result not found" });
        }

        _mapper.Map(updateTestResultDto, testResult);
        await _context.SaveChangesAsync();

        var testResultDto = _mapper.Map<QualityTestResultDto>(testResult);
        return Ok(new ApiResponse<QualityTestResultDto> { Data = testResultDto, Success = true, Message = "Quality test result updated successfully" });
    }

    /// <summary>
    /// Delete quality test result
    /// </summary>
    [HttpDelete("test-results/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteQualityTestResult(Guid id)
    {
        var testResult = await _context.QualityTestResults.FindAsync(id);
        if (testResult == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Quality test result not found" });
        }

        _context.QualityTestResults.Remove(testResult);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Quality test result deleted successfully" });
    }

    #endregion

    #region Seal Records

    /// <summary>
    /// Get seal records
    /// </summary>
    [HttpGet("seals")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SealRecordDto>>>> GetSealRecords(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.SealRecords.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(sr => sr.SealNumber.Contains(queryParams.Search) || sr.SealType.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var sealRecords = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var sealRecordDtos = _mapper.Map<IEnumerable<SealRecordDto>>(sealRecords);

        return Ok(new ApiResponse<IEnumerable<SealRecordDto>>
        {
            Data = sealRecordDtos,
            Success = true,
            Message = "Seal records retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get seal record by ID
    /// </summary>
    [HttpGet("seals/{id}")]
    public async Task<ActionResult<ApiResponse<SealRecordDto>>> GetSealRecord(Guid id)
    {
        var sealRecord = await _context.SealRecords.FindAsync(id);
        if (sealRecord == null)
        {
            return NotFound(new ApiResponse<SealRecordDto> { Success = false, Message = "Seal record not found" });
        }

        var sealRecordDto = _mapper.Map<SealRecordDto>(sealRecord);
        return Ok(new ApiResponse<SealRecordDto> { Data = sealRecordDto, Success = true, Message = "Seal record retrieved successfully" });
    }

    /// <summary>
    /// Get seal records by vehicle ID
    /// </summary>
    [HttpGet("seals/by-vehicle/{vehicleId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SealRecordDto>>>> GetSealRecordsByVehicle(Guid vehicleId)
    {
        var sealRecords = await _context.SealRecords
            .Where(sr => sr.VehicleId == vehicleId)
            .ToListAsync();

        var sealRecordDtos = _mapper.Map<IEnumerable<SealRecordDto>>(sealRecords);
        return Ok(new ApiResponse<IEnumerable<SealRecordDto>> 
        { 
            Data = sealRecordDtos, 
            Success = true, 
            Message = "Seal records retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new seal record
    /// </summary>
    [HttpPost("seals")]
    public async Task<ActionResult<ApiResponse<SealRecordDto>>> CreateSealRecord(CreateSealRecordDto createSealRecordDto)
    {
        var sealRecord = _mapper.Map<SealRecord>(createSealRecordDto);
        _context.SealRecords.Add(sealRecord);
        await _context.SaveChangesAsync();

        var sealRecordDto = _mapper.Map<SealRecordDto>(sealRecord);
        return CreatedAtAction(nameof(GetSealRecord), new { id = sealRecord.Id },
            new ApiResponse<SealRecordDto> { Data = sealRecordDto, Success = true, Message = "Seal record created successfully" });
    }

    /// <summary>
    /// Update seal record
    /// </summary>
    [HttpPut("seals/{id}")]
    public async Task<ActionResult<ApiResponse<SealRecordDto>>> UpdateSealRecord(Guid id, UpdateSealRecordDto updateSealRecordDto)
    {
        var sealRecord = await _context.SealRecords.FindAsync(id);
        if (sealRecord == null)
        {
            return NotFound(new ApiResponse<SealRecordDto> { Success = false, Message = "Seal record not found" });
        }

        _mapper.Map(updateSealRecordDto, sealRecord);
        await _context.SaveChangesAsync();

        var sealRecordDto = _mapper.Map<SealRecordDto>(sealRecord);
        return Ok(new ApiResponse<SealRecordDto> { Data = sealRecordDto, Success = true, Message = "Seal record updated successfully" });
    }

    /// <summary>
    /// Delete seal record
    /// </summary>
    [HttpDelete("seals/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSealRecord(Guid id)
    {
        var sealRecord = await _context.SealRecords.FindAsync(id);
        if (sealRecord == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Seal record not found" });
        }

        _context.SealRecords.Remove(sealRecord);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Seal record deleted successfully" });
    }

    #endregion

    #region Vehicle Incidents

    /// <summary>
    /// Get vehicle incidents
    /// </summary>
    [HttpGet("incidents")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleIncidentDto>>>> GetVehicleIncidents(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.VehicleIncidents.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(vi => vi.IncidentType.Contains(queryParams.Search) || vi.Description.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var incidents = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var incidentDtos = _mapper.Map<IEnumerable<VehicleIncidentDto>>(incidents);

        return Ok(new ApiResponse<IEnumerable<VehicleIncidentDto>>
        {
            Data = incidentDtos,
            Success = true,
            Message = "Vehicle incidents retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get vehicle incident by ID
    /// </summary>
    [HttpGet("incidents/{id}")]
    public async Task<ActionResult<ApiResponse<VehicleIncidentDto>>> GetVehicleIncident(Guid id)
    {
        var incident = await _context.VehicleIncidents.FindAsync(id);
        if (incident == null)
        {
            return NotFound(new ApiResponse<VehicleIncidentDto> { Success = false, Message = "Vehicle incident not found" });
        }

        var incidentDto = _mapper.Map<VehicleIncidentDto>(incident);
        return Ok(new ApiResponse<VehicleIncidentDto> { Data = incidentDto, Success = true, Message = "Vehicle incident retrieved successfully" });
    }

    /// <summary>
    /// Get vehicle incidents by vehicle ID
    /// </summary>
    [HttpGet("incidents/by-vehicle/{vehicleId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleIncidentDto>>>> GetVehicleIncidentsByVehicle(Guid vehicleId)
    {
        var incidents = await _context.VehicleIncidents
            .Where(vi => vi.VehicleId == vehicleId)
            .ToListAsync();

        var incidentDtos = _mapper.Map<IEnumerable<VehicleIncidentDto>>(incidents);
        return Ok(new ApiResponse<IEnumerable<VehicleIncidentDto>> 
        { 
            Data = incidentDtos, 
            Success = true, 
            Message = "Vehicle incidents retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new vehicle incident
    /// </summary>
    [HttpPost("incidents")]
    public async Task<ActionResult<ApiResponse<VehicleIncidentDto>>> CreateVehicleIncident(CreateVehicleIncidentDto createIncidentDto)
    {
        var incident = _mapper.Map<VehicleIncident>(createIncidentDto);
        _context.VehicleIncidents.Add(incident);
        await _context.SaveChangesAsync();

        var incidentDto = _mapper.Map<VehicleIncidentDto>(incident);
        return CreatedAtAction(nameof(GetVehicleIncident), new { id = incident.Id },
            new ApiResponse<VehicleIncidentDto> { Data = incidentDto, Success = true, Message = "Vehicle incident created successfully" });
    }

    /// <summary>
    /// Update vehicle incident
    /// </summary>
    [HttpPut("incidents/{id}")]
    public async Task<ActionResult<ApiResponse<VehicleIncidentDto>>> UpdateVehicleIncident(Guid id, UpdateVehicleIncidentDto updateIncidentDto)
    {
        var incident = await _context.VehicleIncidents.FindAsync(id);
        if (incident == null)
        {
            return NotFound(new ApiResponse<VehicleIncidentDto> { Success = false, Message = "Vehicle incident not found" });
        }

        _mapper.Map(updateIncidentDto, incident);
        await _context.SaveChangesAsync();

        var incidentDto = _mapper.Map<VehicleIncidentDto>(incident);
        return Ok(new ApiResponse<VehicleIncidentDto> { Data = incidentDto, Success = true, Message = "Vehicle incident updated successfully" });
    }

    /// <summary>
    /// Delete vehicle incident
    /// </summary>
    [HttpDelete("incidents/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicleIncident(Guid id)
    {
        var incident = await _context.VehicleIncidents.FindAsync(id);
        if (incident == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Vehicle incident not found" });
        }

        _context.VehicleIncidents.Remove(incident);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Vehicle incident deleted successfully" });
    }

    #endregion
}