using Microsoft.AspNetCore.Mvc;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Interfaces;

namespace OperationalDataService.Api.Controllers;

/// <summary>
/// Controller for managing product catalog operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/products")]
// [ApiVersion("1.0")]
public class ProductManagementController : BaseController
{
    private readonly IProductCatalogService _productService;
    private readonly ILogger<ProductManagementController> _logger;

    public ProductManagementController(
        IProductCatalogService productService,
        ILogger<ProductManagementController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active products for the organization
    /// </summary>
    /// <returns>List of active products</returns>
    [HttpGet]
    public async Task<IActionResult> GetActiveProducts()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var products = await _productService.GetActiveProductsAsync(organizationId);
            return Ok(products, "Active products retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active products");
            return InternalServerError("Failed to retrieve active products");
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            return Ok(product, "Product retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return InternalServerError("Failed to retrieve product");
        }
    }

    /// <summary>
    /// Sync a product from master data service
    /// </summary>
    /// <param name="request">Sync product request</param>
    /// <returns>Synced product data</returns>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncProduct([FromBody] SyncProductRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProductId))
            {
                return BadRequest("Product ID is required");
            }

            var organizationId = GetOrganizationId();
            var product = await _productService.SyncProductAsync(request.ProductId, organizationId);
            
            return Ok(product, "Product synced successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product sync failed for {ProductId}", request.ProductId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing product {ProductId}", request.ProductId);
            return InternalServerError("Failed to sync product");
        }
    }

    /// <summary>
    /// Get current pricing for a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="date">Pricing date (optional, defaults to current date)</param>
    /// <returns>Product pricing information</returns>
    [HttpGet("{id}/pricing")]
    public async Task<IActionResult> GetProductPricing(string id, [FromQuery] DateTime? date = null)
    {
        try
        {
            var pricingDate = date ?? DateTime.UtcNow;
            var pricing = await _productService.GetProductPricingAsync(id, pricingDate);
            
            return Ok(pricing, "Product pricing retrieved successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product pricing not found for {ProductId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product pricing for {ProductId}", id);
            return InternalServerError("Failed to retrieve product pricing");
        }
    }

    /// <summary>
    /// Update product availability
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Update availability request</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/availability")]
    public async Task<IActionResult> UpdateProductAvailability(string id, [FromBody] UpdateProductAvailabilityRequest request)
    {
        try
        {
            await _productService.UpdateProductAvailabilityAsync(
                id, 
                request.Quantity, 
                request.Operation, 
                request.Reason);
            
            return Ok("Availability updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product availability update failed for {ProductId}", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for product availability update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product availability for {ProductId}", id);
            return InternalServerError("Failed to update product availability");
        }
    }

    /// <summary>
    /// Validate product for transaction
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="vehicleId">Vehicle ID (optional)</param>
    /// <returns>Product validation result</returns>
    [HttpGet("{id}/validate")]
    public async Task<IActionResult> ValidateProduct(string id, [FromQuery] string? vehicleId = null)
    {
        try
        {
            var validationResult = await _productService.ValidateProductForTransactionAsync(id, vehicleId ?? string.Empty);
            return Ok(validationResult, "Product validation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product {ProductId}", id);
            return InternalServerError("Failed to validate product");
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <returns>List of products in the category</returns>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var products = await _productService.GetProductsByCategoryAsync(category, organizationId);
            return Ok(products, $"Products in category '{category}' retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by category {Category}", category);
            return InternalServerError("Failed to retrieve products by category");
        }
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    /// <returns>List of products with low stock</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var products = await _productService.GetLowStockProductsAsync(organizationId);
            return Ok(products, "Low stock products retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock products");
            return InternalServerError("Failed to retrieve low stock products");
        }
    }

    /// <summary>
    /// Search products
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>List of matching products</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required");
            }

            var organizationId = GetOrganizationId();
            var products = await _productService.SearchProductsAsync(query, organizationId);
            return Ok(products, $"Search results for '{query}' retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query {Query}", query);
            return InternalServerError("Failed to search products");
        }
    }

    /// <summary>
    /// Check product availability
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="quantity">Required quantity</param>
    /// <returns>Availability status</returns>
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> CheckProductAvailability(string id, [FromQuery] decimal quantity)
    {
        try
        {
            var isAvailable = await _productService.IsProductAvailableAsync(id, quantity);
            return Ok(new { ProductId = id, RequiredQuantity = quantity, IsAvailable = isAvailable }, 
                "Product availability checked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product availability for {ProductId}", id);
            return InternalServerError("Failed to check product availability");
        }
    }

    /// <summary>
    /// Get products by supplier
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>List of products from the supplier</returns>
    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetProductsBySupplier(string supplierId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var products = await _productService.GetProductsBySupplierAsync(supplierId, organizationId);
            return Ok(products, $"Products from supplier '{supplierId}' retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by supplier {SupplierId}", supplierId);
            return InternalServerError("Failed to retrieve products by supplier");
        }
    }

    /// <summary>
    /// Refresh product catalog from master data
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshProductCatalog()
    {
        try
        {
            var organizationId = GetOrganizationId();
            await _productService.RefreshProductCatalogAsync(organizationId);
            return Ok("Product catalog refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing product catalog");
            return InternalServerError("Failed to refresh product catalog");
        }
    }

    /// <summary>
    /// Update a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="productData">Updated product data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductCatalogDto productData)
    {
        try
        {
            if (id != productData.ProductId)
            {
                return BadRequest("Product ID in URL does not match the product data");
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, productData);
            return Ok(updatedProduct, "Product updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product update failed for {ProductId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return InternalServerError("Failed to update product");
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        try
        {
            var success = await _productService.DeleteProductAsync(id);
            
            if (!success)
            {
                return NotFound($"Product with ID {id} not found");
            }

            return Ok("Product deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return InternalServerError("Failed to delete product");
        }
    }
}