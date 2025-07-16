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
        var product = _mapper.Map<ProductService.Core.Entities.Product>(dto);
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
}