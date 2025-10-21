using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Product;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.Core.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IRepository<Product> productRepository,
        IMapper mapper)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<ProductDto>> GetPagedProductsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        // Ensure page number and size are valid
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Get paginated products with search
        var pagedResult = await _productRepository.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: searchTerm,
            searchProperties: new[] { nameof(Entities.Product.Name), nameof(Entities.Product.Code) }
        );

        if (!pagedResult.Items.Any())
        {
            return new PagedResult<ProductDto>
            {
                Items = Enumerable.Empty<ProductDto>(),
                TotalItems = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        var items = pagedResult.Items.Select(p => _mapper.Map<ProductDto>(p)).ToList();

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalItems = pagedResult.TotalItems,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var products = await _productRepository.GetByIdsAsync(new[] { id.ToString() });
        var product = products.FirstOrDefault();
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await IsCodeAvailableAsync(dto.Code) == false)
        {
            throw new InvalidOperationException($"A product with code '{dto.Code}' already exists.");
        }

        var product = _mapper.Map<Entities.Product>(dto);
        var createdProduct = await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var products = await _productRepository.GetByIdsAsync(new[] { id.ToString() });
        var existingProduct = products.FirstOrDefault();
        if (existingProduct == null)
        {
            return null;
        }

        if (await IsCodeAvailableAsync(dto.Code, id) == false)
        {
            throw new InvalidOperationException($"A product with code '{dto.Code}' already exists.");
        }

        _mapper.Map(dto, existingProduct);
        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return updatedProduct != null ? _mapper.Map<ProductDto>(updatedProduct) : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _productRepository.DeleteAsync(id.ToString());
    }

    public async Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        // Get all products with the same code (case-insensitive)
        var result = await _productRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            searchTerm: code,
            searchProperties: new[] { nameof(Entities.Product.Code) }
        );

        // If no product found with this code, it's available
        if (!result.Items.Any())
        {
            return true;
        }

        // If checking for a specific product (update case), exclude it from the check
        if (excludeId.HasValue)
        {
            return result.Items.All(p => p.Id == excludeId.Value.ToString());
        }

        return false;
    }
}
