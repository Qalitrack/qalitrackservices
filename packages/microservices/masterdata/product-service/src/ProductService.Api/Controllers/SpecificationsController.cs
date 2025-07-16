using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecificationsController : BaseController
{
    private readonly ISpecificationService _specificationService;

    public SpecificationsController(ISpecificationService specificationService)
    {
        _specificationService = specificationService;
    }

    /// <summary>
    /// Get all specifications
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<SpecificationDto>>>> GetAll()
    {
        try
        {
            var specifications = await _specificationService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<SpecificationDto>>.Success(specifications));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get specification by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<SpecificationDto>>> GetById(string id)
    {
        try
        {
            var specification = await _specificationService.GetByIdAsync(id);
            if (specification == null)
            {
                return NotFound(ApiResponseDto<SpecificationDto>.Error("Specification not found"));
            }

            return Ok(ApiResponseDto<SpecificationDto>.Success(specification));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get specifications for a specific product
    /// </summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<SpecificationDto>>>> GetByProductId(string productId)
    {
        try
        {
            var specifications = await _specificationService.GetByProductIdAsync(productId);
            return Ok(ApiResponseDto<IEnumerable<SpecificationDto>>.Success(specifications));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get compliance specifications for a product
    /// </summary>
    [HttpGet("product/{productId}/compliance")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<SpecificationDto>>>> GetComplianceSpecifications(string productId)
    {
        try
        {
            var specifications = await _specificationService.GetComplianceSpecificationsAsync(productId);
            return Ok(ApiResponseDto<IEnumerable<SpecificationDto>>.Success(specifications));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate compliance for a product
    /// </summary>
    [HttpGet("product/{productId}/compliance/validate")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ValidateCompliance(string productId)
    {
        try
        {
            var isCompliant = await _specificationService.ValidateComplianceAsync(productId);
            return Ok(ApiResponseDto<bool>.Success(isCompliant));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get specifications with expiring certifications
    /// </summary>
    [HttpGet("expiring-certifications")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<SpecificationDto>>>> GetExpiringCertifications([FromQuery] int daysAhead = 30)
    {
        try
        {
            var specifications = await _specificationService.GetExpiringCertificationsAsync(daysAhead);
            return Ok(ApiResponseDto<IEnumerable<SpecificationDto>>.Success(specifications));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new specification
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<SpecificationDto>>> Create([FromBody] SpecificationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDto<SpecificationDto>.Error("Invalid model state", GetModelStateErrors()));
            }

            var specification = await _specificationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = specification.Id }, 
                ApiResponseDto<SpecificationDto>.Success(specification));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update an existing specification
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<SpecificationDto>>> Update(string id, [FromBody] SpecificationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDto<SpecificationDto>.Error("Invalid model state", GetModelStateErrors()));
            }

            var specification = await _specificationService.UpdateAsync(id, dto);
            if (specification == null)
            {
                return NotFound(ApiResponseDto<SpecificationDto>.Error("Specification not found"));
            }

            return Ok(ApiResponseDto<SpecificationDto>.Success(specification));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a specification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(string id)
    {
        try
        {
            var result = await _specificationService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponseDto<bool>.Error("Specification not found"));
            }

            return Ok(ApiResponseDto<bool>.Success(true));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}