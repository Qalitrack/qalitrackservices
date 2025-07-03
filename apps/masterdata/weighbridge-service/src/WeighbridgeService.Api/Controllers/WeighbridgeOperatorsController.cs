using Microsoft.AspNetCore.Mvc;
using WeighbridgeService.Core.DTOs;
using WeighbridgeService.Core.Interfaces;

namespace WeighbridgeService.Api.Controllers;

[ApiController]
[Route("api/weighbridges/{weighbridgeId}/operators")]
public class WeighbridgeOperatorsController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<WeighbridgeOperatorsController> _logger;

    public WeighbridgeOperatorsController(IWeighbridgeService weighbridgeService, ILogger<WeighbridgeOperatorsController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeOperatorDto>>>> GetAssignedOperators(string weighbridgeId)
    {
        try
        {
            var operators = await _weighbridgeService.GetAssignedOperatorsAsync(weighbridgeId);
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeOperatorDto>>.SuccessResponse(operators));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving operators for weighbridge {WeighbridgeId}", weighbridgeId);
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeOperatorDto>>.ErrorResponse("An error occurred while retrieving operators"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<WeighbridgeOperatorDto>>> AssignOperator(
        string weighbridgeId, [FromBody] AssignOperatorRequest request)
    {
        try
        {
            var operatorDto = await _weighbridgeService.AssignOperatorAsync(weighbridgeId, request);
            return CreatedAtAction(nameof(GetAssignedOperators), new { weighbridgeId }, 
                ApiResponseDto<WeighbridgeOperatorDto>.SuccessResponse(operatorDto, "Operator assigned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning operator to weighbridge {WeighbridgeId}", weighbridgeId);
            return StatusCode(500, ApiResponseDto<WeighbridgeOperatorDto>.ErrorResponse("An error occurred while assigning operator"));
        }
    }

    [HttpDelete("{operatorId}")]
    public async Task<ActionResult<ApiResponseDto<object>>> UnassignOperator(string weighbridgeId, string operatorId)
    {
        try
        {
            await _weighbridgeService.UnassignOperatorAsync(weighbridgeId, operatorId);
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Operator unassigned successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponseDto<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning operator {OperatorId} from weighbridge {WeighbridgeId}", operatorId, weighbridgeId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred while unassigning operator"));
        }
    }
}

[ApiController]
[Route("api/operators")]
public class OperatorsController : ControllerBase
{
    private readonly IWeighbridgeService _weighbridgeService;
    private readonly ILogger<OperatorsController> _logger;

    public OperatorsController(IWeighbridgeService weighbridgeService, ILogger<OperatorsController> logger)
    {
        _weighbridgeService = weighbridgeService;
        _logger = logger;
    }

    [HttpGet("training-needed")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<WeighbridgeOperatorDto>>>> GetOperatorsNeedingTraining()
    {
        try
        {
            var operators = await _weighbridgeService.GetOperatorsNeedingTrainingAsync();
            return Ok(ApiResponseDto<IEnumerable<WeighbridgeOperatorDto>>.SuccessResponse(operators));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving operators needing training");
            return StatusCode(500, ApiResponseDto<IEnumerable<WeighbridgeOperatorDto>>.ErrorResponse("An error occurred while retrieving operators needing training"));
        }
    }
}