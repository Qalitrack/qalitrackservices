using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;

namespace ProductService.Core.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductReadDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductReadDto>>(products);
    }

    public async Task<ProductReadDto?> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<ProductReadDto>(product);
    }

    public async Task<ProductReadDto> CreateAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        var createdProduct = await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductReadDto>(createdProduct);
    }

    public async Task<ProductReadDto?> UpdateAsync(string id, UpdateProductDto dto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return null;
        }

        _mapper.Map(dto, existingProduct);
        existingProduct.UpdatedAt = DateTime.UtcNow;
        
        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return updatedProduct == null ? null : _mapper.Map<ProductReadDto>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _productRepository.IsNameAvailableAsync(name);
    }

    // Inventory Management
    public async Task<bool> IsAvailableAsync(string productId, int quantity = 1)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product != null && product.StockQuantity >= quantity;
    }

    public async Task<int> GetAvailableStockAsync(string productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product?.StockQuantity ?? 0;
    }

    public async Task<bool> UpdateStockAsync(string productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return false;
        
        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _productRepository.UpdateAsync(product);
        return updated != null;
    }

    public async Task<bool> ReserveStockAsync(string productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || product.StockQuantity < quantity) return false;
        
        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _productRepository.UpdateAsync(product);
        return updated != null;
    }

    public async Task<bool> ReleaseStockAsync(string productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return false;
        
        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _productRepository.UpdateAsync(product);
        return updated != null;
    }

    public async Task<IEnumerable<ProductReadDto>> GetLowStockProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var lowStockProducts = products.Where(p => p.StockQuantity <= p.MinimumStock).ToList();
        return _mapper.Map<IEnumerable<ProductReadDto>>(lowStockProducts);
    }

    // Search and Filtering
    public async Task<IEnumerable<ProductReadDto>> SearchAsync(string searchTerm)
    {
        var products = await _productRepository.GetAllAsync();
        var filteredProducts = products.Where(p => 
            p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();
        
        return _mapper.Map<IEnumerable<ProductReadDto>>(filteredProducts);
    }

    public async Task<IEnumerable<ProductReadDto>> GetByCategoryAsync(string categoryId)
    {
        var products = await _productRepository.GetAllAsync();
        var categoryProducts = products.Where(p => p.CategoryId == categoryId).ToList();
        return _mapper.Map<IEnumerable<ProductReadDto>>(categoryProducts);
    }

    public async Task<IEnumerable<ProductReadDto>> GetByStatusAsync(ProductStatus status)
    {
        var products = await _productRepository.GetAllAsync();
        var statusProducts = products.Where(p => p.Status == status).ToList();
        return _mapper.Map<IEnumerable<ProductReadDto>>(statusProducts);
    }

    // Integration with Customer Service
    public async Task<decimal> GetCustomerPriceAsync(string productId, string customerId, int quantity = 1)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return 0;
        
        // Basic implementation - should be enhanced with pricing service integration
        // For now, return a default price of 0 as Product entity doesn't have BasePrice
        return 0;
    }

    public async Task<IEnumerable<ProductReadDto>> GetCustomerProductsAsync(string customerId)
    {
        // Basic implementation - return all active products
        var products = await _productRepository.GetAllAsync();
        var activeProducts = products.Where(p => p.Status == ProductStatus.Active).ToList();
        return _mapper.Map<IEnumerable<ProductReadDto>>(activeProducts);
    }
}