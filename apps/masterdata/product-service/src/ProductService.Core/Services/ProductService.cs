using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;

namespace ProductService.Core.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IProductComplianceRepository _complianceRepository;
    private readonly IRepository<ProductSpecification> _specificationRepository;
    private readonly IRepository<ProductPricing> _pricingRepository;
    private readonly IRepository<ProductHazmat> _hazmatRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        IProductCategoryRepository categoryRepository,
        IProductComplianceRepository complianceRepository,
        IRepository<ProductSpecification> specificationRepository,
        IRepository<ProductPricing> pricingRepository,
        IRepository<ProductHazmat> hazmatRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _complianceRepository = complianceRepository;
        _specificationRepository = specificationRepository;
        _pricingRepository = pricingRepository;
        _hazmatRepository = hazmatRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto> RegisterProductAsync(RegisterProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitOfMeasure = request.UnitOfMeasure,
            Weight = request.Weight,
            Density = request.Density,
            IsHazardous = request.IsHazardous,
            HazmatClass = request.HazmatClass,
            RequiresSpecialHandling = request.RequiresSpecialHandling,
            Status = ProductStatus.Active,
            Notes = request.Notes
        };

        var createdProduct = await _productRepository.AddAsync(product);
        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new ArgumentException("Product not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.UnitOfMeasure = request.UnitOfMeasure;
        product.Weight = request.Weight;
        product.Density = request.Density;
        product.IsHazardous = request.IsHazardous;
        product.HazmatClass = request.HazmatClass;
        product.RequiresSpecialHandling = request.RequiresSpecialHandling;
        product.Status = Enum.Parse<ProductStatus>(request.Status);
        product.Notes = request.Notes;
        product.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<ProductDto?> GetProductByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<ProductDto?> GetProductByCodeAsync(string code)
    {
        var product = await _productRepository.GetByCodeAsync(code);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(string categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<List<ProductDto>> GetHazardousProductsAsync()
    {
        var products = await _productRepository.GetHazardousProductsAsync();
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _productRepository.SearchProductsAsync(searchTerm);
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task DeleteProductAsync(string id)
    {
        await _productRepository.DeleteAsync(id);
    }

    public async Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequest request)
    {
        var category = new ProductCategory
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        var createdCategory = await _categoryRepository.AddAsync(category);
        return _mapper.Map<ProductCategoryDto>(createdCategory);
    }

    public async Task<List<ProductCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<List<ProductCategoryDto>>(categories);
    }

    public async Task<List<ProductCategoryDto>> GetRootCategoriesAsync()
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync();
        return _mapper.Map<List<ProductCategoryDto>>(categories);
    }

    public async Task<List<ProductCategoryDto>> GetSubCategoriesAsync(string parentCategoryId)
    {
        var categories = await _categoryRepository.GetSubCategoriesAsync(parentCategoryId);
        return _mapper.Map<List<ProductCategoryDto>>(categories);
    }

    public async Task<List<ProductSpecificationDto>> GetProductSpecificationsAsync(string productId)
    {
        var specifications = await _specificationRepository.GetAllAsync();
        var productSpecs = specifications.Where(s => s.Id == productId);
        return _mapper.Map<List<ProductSpecificationDto>>(productSpecs);
    }

    public async Task UpdateProductSpecificationsAsync(string productId, UpdateProductSpecificationsRequest request)
    {
        // This would typically involve more complex logic to handle updates, additions, and deletions
        // For now, we'll implement a simple approach
        foreach (var specDto in request.Specifications)
        {
            if (string.IsNullOrEmpty(specDto.Id))
            {
                // Create new specification
                var newSpec = _mapper.Map<ProductSpecification>(specDto);
                newSpec.ProductId = productId;
                await _specificationRepository.AddAsync(newSpec);
            }
            else
            {
                // Update existing specification
                var existingSpec = await _specificationRepository.GetByIdAsync(specDto.Id);
                if (existingSpec != null)
                {
                    _mapper.Map(specDto, existingSpec);
                    await _specificationRepository.UpdateAsync(existingSpec);
                }
            }
        }
    }

    public async Task<List<ProductPricingDto>> GetProductPricingAsync(string productId)
    {
        var pricing = await _pricingRepository.GetAllAsync();
        var productPricing = pricing.Where(p => p.Id == productId);
        return _mapper.Map<List<ProductPricingDto>>(productPricing);
    }

    public async Task UpdateProductPricingAsync(string productId, UpdateProductPricingRequest request)
    {
        // Similar logic as specifications
        foreach (var pricingDto in request.PricingRules)
        {
            if (string.IsNullOrEmpty(pricingDto.Id))
            {
                var newPricing = _mapper.Map<ProductPricing>(pricingDto);
                newPricing.ProductId = productId;
                await _pricingRepository.AddAsync(newPricing);
            }
            else
            {
                var existingPricing = await _pricingRepository.GetByIdAsync(pricingDto.Id);
                if (existingPricing != null)
                {
                    _mapper.Map(pricingDto, existingPricing);
                    await _pricingRepository.UpdateAsync(existingPricing);
                }
            }
        }
    }

    public async Task<ProductComplianceDto> GetProductComplianceAsync(string productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new ArgumentException("Product not found");

        var compliance = await _complianceRepository.GetByProductIdAsync(productId);
        var hazmatInfo = await _hazmatRepository.GetByIdAsync(productId);

        return new ProductComplianceDto
        {
            ProductId = productId,
            IsHazardous = product.IsHazardous,
            HazmatClass = product.HazmatClass,
            TransportRequirements = hazmatInfo?.TransportRequirements,
            StorageRequirements = hazmatInfo?.StorageRequirements,
            HandlingInstructions = hazmatInfo?.HandlingInstructions,
            EmergencyProcedures = hazmatInfo?.EmergencyProcedures,
            Requirements = _mapper.Map<List<ComplianceRequirementDto>>(compliance)
        };
    }
}