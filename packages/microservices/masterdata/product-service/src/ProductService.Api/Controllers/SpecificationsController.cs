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
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var specifications = await _specificationService.GetAllAsync();
            return Ok(specifications);
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
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var specification = await _specificationService.GetByIdAsync(id);
            if (specification == null)
            {
                return NotFound("Specification not found");
            }

            return Ok(specification);
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
    public async Task<IActionResult> GetByProductId(string productId)
    {
        try
        {
            var specifications = await _specificationService.GetByProductIdAsync(productId);
            return Ok(specifications);
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
    public async Task<IActionResult> GetComplianceSpecifications(string productId)
    {
        try
        {
            var specifications = await _specificationService.GetComplianceSpecificationsAsync(productId);
            return Ok(specifications);
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
    public async Task<IActionResult> ValidateCompliance(string productId)
    {
        try
        {
            var isCompliant = await _specificationService.ValidateComplianceAsync(productId);
            return Ok(isCompliant);
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
    public async Task<IActionResult> GetExpiringCertifications([FromQuery] int daysAhead = 30)
    {
        try
        {
            var specifications = await _specificationService.GetExpiredCertificationsAsync();
            return Ok(specifications);
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
    public async Task<IActionResult> Create([FromBody] CreateSpecificationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state", GetModelStateErrors());
            }

            var specification = await _specificationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = specification.Id }, specification);
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
    public async Task<IActionResult> Update(string id, [FromBody] UpdateSpecificationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state", GetModelStateErrors());
            }

            var specification = await _specificationService.UpdateAsync(id, dto);
            if (specification == null)
            {
                return NotFound("Specification not found");
            }

            return Ok(specification);
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
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _specificationService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Specification not found");
            }

            return Ok(true);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}