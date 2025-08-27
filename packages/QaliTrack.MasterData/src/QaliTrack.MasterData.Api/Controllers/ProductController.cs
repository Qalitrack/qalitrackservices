using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Product.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Product Module")]
public class ProductController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public ProductController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get products with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Product>>>> GetProducts(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Specifications)
                .Include(p => p.Documents)
                .Include(p => p.Pricing)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<Product>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Product>>.ErrorResponse("Error retrieving products", ex.Message));
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Product>>> GetProduct(Guid id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Specifications)
                .Include(p => p.Documents)
                .Include(p => p.Pricing)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(ApiResponse<Product>.ErrorResponse("Product not found"));
            }

            return Ok(ApiResponse<Product>.SuccessResponse(product));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Product>.ErrorResponse("Error retrieving product", ex.Message));
        }
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Product>>> CreateProduct(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), 
                new { id = product.Id }, 
                ApiResponse<Product>.SuccessResponse(product, "Product created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Product>.ErrorResponse("Error creating product", ex.Message));
        }
    }

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Product>>> UpdateProduct(Guid id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(ApiResponse<Product>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Product>.SuccessResponse(product, "Product updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(id))
            {
                return NotFound(ApiResponse<Product>.ErrorResponse("Product not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Product>.ErrorResponse("Error updating product", ex.Message));
        }
    }

    /// <summary>
    /// Delete product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.CreateError("Product not found"));
            }

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Product deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting product", ex.Message));
        }
    }

    private async Task<bool> ProductExists(Guid id)
    {
        return await _context.Products.AnyAsync(e => e.Id == id);
    }
}