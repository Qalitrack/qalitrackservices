using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;

namespace ProductService.Api.Controllers;

[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;
    private readonly IPricingService _pricingService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, IPricingService pricingService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _pricingService = pricingService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return InternalServerError("An error occurred while retrieving products");
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with id {Id}", id);
            return InternalServerError("An error occurred while retrieving product");
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto request)
    {
        try
        {
            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return InternalServerError("An error occurred while creating product");
        }
    }

    /// <summary>
    /// Register a new product (integration compatibility)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterProductRequest request)
    {
        try
        {
            // Convert RegisterProductRequest to CreateProductDto
            var createDto = new CreateProductDto
            {
                Name = request.Name,
                Description = request.Description,
                Code = GenerateProductCode(request.Name),
                SKU = GenerateProductSKU(request.Name),
                Brand = "Default Brand",
                CategoryId = GetOrCreateCategoryId(request.Category)
            };

            var product = await _productService.CreateAsync(createDto);
            
            // Create base pricing
            if (request.BasePrice > 0)
            {
                var pricingDto = new CreatePricingDto
                {
                    ProductId = product.Id,
                    Type = Core.Entities.PricingType.Standard,
                    BasePrice = request.BasePrice,
                    Currency = request.Currency,
                    ValidFrom = DateTime.UtcNow,
                    IsActive = true
                };
                await _pricingService.CreateAsync(pricingDto);
            }

            // Convert to ProductDto for response
            var responseDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = request.Category,
                UnitOfMeasure = request.UnitOfMeasure,
                BasePrice = request.BasePrice,
                Currency = request.Currency,
                Code = product.Code,
                SKU = product.SKU,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering product");
            return InternalServerError("An error occurred while registering product");
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProductDto request)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, request);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(product, "Product updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with id {Id}", id);
            return InternalServerError("An error occurred while updating product");
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _productService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Product not found");
            }

            return Ok<object?>(null, "Product deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with id {Id}", id);
            return InternalServerError("An error occurred while deleting product");
        }
    }

    /// <summary>
    /// Check if product name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _productService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }

    /// <summary>
    /// Get product pricing (integration compatibility)
    /// </summary>
    [HttpGet("{id}/pricing")]
    public async Task<IActionResult> GetPricing(string id)
    {
        try
        {
            var pricings = await _pricingService.GetByProductIdAsync(id);
            var pricingDtos = pricings.Select(p => new ProductPricingDto
            {
                Id = p.Id,
                ProductId = p.ProductId,
                PricingType = p.Type.ToString(),
                Price = p.EffectivePrice,
                Currency = p.Currency,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                IsActive = p.IsActive
            }).ToList();

            // Add base pricing if no pricing exists
            if (!pricingDtos.Any())
            {
                var product = await _productService.GetByIdAsync(id);
                if (product != null)
                {
                    pricingDtos.Add(new ProductPricingDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = id,
                        PricingType = "Base",
                        Price = 0, // Default base price
                        Currency = "KES",
                        ValidFrom = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            return Ok(pricingDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing for product {Id}", id);
            return InternalServerError("An error occurred while retrieving product pricing");
        }
    }

    private string GenerateProductCode(string name)
    {
        // Generate a simple code from the product name
        var code = string.Concat(name.Where(char.IsLetterOrDigit)).ToUpper();
        if (code.Length > 10) code = code.Substring(0, 10);
        return $"{code}{DateTime.UtcNow.Ticks % 1000:D3}";
    }

    private string GenerateProductSKU(string name)
    {
        // Generate a simple SKU from the product name
        var sku = string.Concat(name.Where(char.IsLetterOrDigit)).ToUpper();
        if (sku.Length > 8) sku = sku.Substring(0, 8);
        return $"SKU-{sku}-{DateTime.UtcNow.Ticks % 10000:D4}";
    }

    private string GetOrCreateCategoryId(string categoryName)
    {
        // For now, return empty string - in a full implementation, 
        // this would create or find the category
        return string.Empty;
    }
}