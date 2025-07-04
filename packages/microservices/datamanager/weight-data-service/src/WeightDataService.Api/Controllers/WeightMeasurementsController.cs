using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Weight measurements management
/// </summary>
[ApiController]
[Route("api/measurements")]
public class WeightMeasurementsController : BaseController
{
    private readonly IWeightMeasurementService _measurementService;

    public WeightMeasurementsController(IWeightMeasurementService measurementService)
    {
        _measurementService = measurementService;
    }

    /// <summary>
    /// Get paginated list of weight measurements
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMeasurements([FromQuery] SearchFilterDto filter)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var result = await _measurementService.GetMeasurementsAsync(organizationId, filter);
            return HandleResult(Success(result, "Measurements retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<PagedResultDto<WeightMeasurementDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get weight measurement by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeasurement(Guid id)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var measurement = await _measurementService.GetMeasurementByIdAsync(id, organizationId);
            
            if (measurement == null)
            {
                return NotFound(Error<WeightMeasurementDto>("Measurement not found"));
            }

            return HandleResult(Success(measurement, "Measurement retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeightMeasurementDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get weight measurement by ticket reference
    /// </summary>
    [HttpGet("ticket/{ticketReference}")]
    public async Task<IActionResult> GetMeasurementByTicket(string ticketReference)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var measurement = await _measurementService.GetMeasurementByTicketAsync(ticketReference, organizationId);
            
            if (measurement == null)
            {
                return NotFound(Error<WeightMeasurementDto>("Measurement not found"));
            }

            return HandleResult(Success(measurement, "Measurement retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeightMeasurementDto>(ex.Message));
        }
    }

    /// <summary>
    /// Create new weight measurement
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMeasurement([FromBody] CreateWeightMeasurementDto createDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var measurement = await _measurementService.CreateMeasurementAsync(createDto, organizationId, userId);
            
            return CreatedAtAction(nameof(GetMeasurement), new { id = measurement.Id }, 
                Success(measurement, "Measurement created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return HandleResult(Error<WeightMeasurementDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeightMeasurementDto>(ex.Message));
        }
    }

    /// <summary>
    /// Update weight measurement
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeasurement(Guid id, [FromBody] UpdateWeightMeasurementDto updateDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var measurement = await _measurementService.UpdateMeasurementAsync(id, updateDto, organizationId, userId);
            
            if (measurement == null)
            {
                return NotFound(Error<WeightMeasurementDto>("Measurement not found"));
            }

            return HandleResult(Success(measurement, "Measurement updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<WeightMeasurementDto>(ex.Message));
        }
    }

    /// <summary>
    /// Delete weight measurement
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeasurement(Guid id)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var deleted = await _measurementService.DeleteMeasurementAsync(id, organizationId, userId);
            
            if (!deleted)
            {
                return NotFound(Error<bool>("Measurement not found"));
            }

            return HandleResult(Success(true, "Measurement deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }

    /// <summary>
    /// Get pending measurements
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingMeasurements()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var measurements = await _measurementService.GetPendingMeasurementsAsync(organizationId);
            
            return HandleResult(Success(measurements, "Pending measurements retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeightMeasurementDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get measurements by vehicle registration
    /// </summary>
    [HttpGet("vehicle/{vehicleRegistration}")]
    public async Task<IActionResult> GetMeasurementsByVehicle(string vehicleRegistration)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var measurements = await _measurementService.GetMeasurementsByVehicleAsync(vehicleRegistration, organizationId);
            
            return HandleResult(Success(measurements, "Vehicle measurements retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeightMeasurementDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get measurements by weighbridge
    /// </summary>
    [HttpGet("weighbridge/{weighbridgeId}")]
    public async Task<IActionResult> GetMeasurementsByWeighbridge(string weighbridgeId, 
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var measurements = await _measurementService.GetMeasurementsByWeighbridgeAsync(weighbridgeId, fromDate, toDate);
            
            return HandleResult(Success(measurements, "Weighbridge measurements retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<WeightMeasurementDto>>(ex.Message));
        }
    }
}