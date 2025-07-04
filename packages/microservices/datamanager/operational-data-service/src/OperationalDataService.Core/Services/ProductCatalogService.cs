using AutoMapper;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;
using OperationalDataService.Core.Interfaces;

namespace OperationalDataService.Core.Services;

public class ProductCatalogService : IProductCatalogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMasterDataIntegrationService _masterDataService;

    public ProductCatalogService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMasterDataIntegrationService masterDataService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _masterDataService = masterDataService;
    }

    public async Task<ProductCatalogDto> SyncProductAsync(string productId, string organizationId)
    {
        // Get product from Master Data Service
        var masterProduct = await _masterDataService.GetProductFromMasterDataAsync(productId);
        if (masterProduct == null)
        {
            throw new InvalidOperationException($"Product {productId} not found in master data");
        }

        // Check if product exists locally
        var existingProduct = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.OrganizationId == organizationId);

        if (existingProduct != null)
        {
            // Update existing product
            existingProduct.Name = masterProduct.Name;
            existingProduct.Description = masterProduct.Description;
            existingProduct.Category = masterProduct.Category;
            existingProduct.Unit = masterProduct.Unit;
            existingProduct.CurrentPrice = masterProduct.CurrentPrice;
            existingProduct.AvailableQuantity = masterProduct.AvailableQuantity;
            existingProduct.ReorderLevel = masterProduct.ReorderLevel;
            existingProduct.Status = masterProduct.Status;
            existingProduct.LastSyncDate = DateTime.UtcNow;
            existingProduct.ComplianceRequirements = masterProduct.ComplianceRequirements;
            existingProduct.QualitySpecifications = masterProduct.QualitySpecifications;
            existingProduct.RequiresSpecialHandling = masterProduct.RequiresSpecialHandling;
            existingProduct.MinOrderQuantity = masterProduct.MinOrderQuantity;
            existingProduct.MaxOrderQuantity = masterProduct.MaxOrderQuantity;
            existingProduct.AllowedVehicleTypes = masterProduct.AllowedVehicleTypes;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ProductCatalog>().UpdateAsync(existingProduct);
        }
        else
        {
            // Create new product
            var newProduct = _mapper.Map<ProductCatalog>(masterProduct);
            newProduct.Id = Guid.NewGuid().ToString();
            newProduct.OrganizationId = organizationId;
            newProduct.LastSyncDate = DateTime.UtcNow;
            newProduct.CreatedAt = DateTime.UtcNow;
            newProduct.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ProductCatalog>().AddAsync(newProduct);
            existingProduct = newProduct;
        }

        await _unitOfWork.SaveChangesAsync();

        // Update pricing if applicable
        await UpdateProductPricingAsync(existingProduct);

        // Validate compliance requirements
        await ValidateProductComplianceAsync(existingProduct);

        return _mapper.Map<ProductCatalogDto>(existingProduct);
    }

    public async Task<List<ProductCatalogDto>> GetActiveProductsAsync(string organizationId)
    {
        var products = await _unitOfWork.Repository<ProductCatalog>()
            .FindAsync(p => p.OrganizationId == organizationId && 
                           p.Status == ProductStatus.Active && 
                           !p.IsDeleted);

        return _mapper.Map<List<ProductCatalogDto>>(products);
    }

    public async Task<ProductValidationResult> ValidateProductForTransactionAsync(string productId, string vehicleId)
    {
        var result = new ProductValidationResult
        {
            ProductId = productId,
            VehicleId = vehicleId,
            ValidatedAt = DateTime.UtcNow
        };

        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            result.IsValid = false;
            result.ValidationErrors.Add("Product not found in catalog");
            return result;
        }

        // Check if product is active
        if (product.Status != ProductStatus.Active)
        {
            result.IsValid = false;
            result.ValidationErrors.Add($"Product is not active. Current status: {product.Status}");
        }

        // Check availability
        if (product.AvailableQuantity <= 0)
        {
            result.IsValid = false;
            result.ValidationErrors.Add("Product is out of stock");
        }
        else if (product.AvailableQuantity <= product.ReorderLevel)
        {
            result.ValidationWarnings.Add("Product stock is below reorder level");
        }

        // Check expiry date
        if (product.ExpiryDate.HasValue && product.ExpiryDate.Value <= DateTime.UtcNow)
        {
            result.IsValid = false;
            result.ValidationErrors.Add("Product has expired");
        }

        // Validate vehicle compatibility
        if (!string.IsNullOrEmpty(vehicleId))
        {
            var vehicleInfo = await _masterDataService.GetVehicleInfoAsync(vehicleId);
            if (vehicleInfo != null)
            {
                if (product.AllowedVehicleTypes.Any() && 
                    !product.AllowedVehicleTypes.Contains(vehicleInfo.Type))
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add($"Vehicle type {vehicleInfo.Type} is not allowed for this product");
                }

                var isVehicleAllowed = await _masterDataService.ValidateVehicleForProductAsync(vehicleId, productId);
                if (!isVehicleAllowed)
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add("Vehicle is not authorized for this product");
                }
            }
        }

        result.Product = _mapper.Map<ProductCatalogDto>(product);
        result.IsValid = !result.ValidationErrors.Any();

        return result;
    }

    public async Task<ProductPricingDto> GetProductPricingAsync(string productId, DateTime date)
    {
        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found");
        }

        // Try to get pricing from master data first
        var masterPricing = await _masterDataService.GetProductPricingFromMasterDataAsync(productId, date);
        if (masterPricing != null)
        {
            return masterPricing;
        }

        // Fall back to local pricing
        return new ProductPricingDto
        {
            ProductId = productId,
            BasePrice = product.CurrentPrice,
            CurrentPrice = product.CurrentPrice,
            PricingDate = date,
            Currency = "USD", // Default currency
            LastUpdated = product.UpdatedAt
        };
    }

    public async Task UpdateProductAvailabilityAsync(string productId, decimal quantity, string operation = "SET", string? reason = null)
    {
        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found");
        }

        var previousQuantity = product.AvailableQuantity;

        switch (operation.ToUpper())
        {
            case "SET":
                product.AvailableQuantity = quantity;
                break;
            case "ADD":
                product.AvailableQuantity += quantity;
                break;
            case "SUBTRACT":
                product.AvailableQuantity -= quantity;
                if (product.AvailableQuantity < 0)
                {
                    product.AvailableQuantity = 0;
                }
                break;
            default:
                throw new ArgumentException($"Invalid operation: {operation}");
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<ProductCatalog>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Log the quantity change if needed
        // This could be extended to maintain an audit trail
    }

    public async Task<ProductCatalogDto?> GetProductByIdAsync(string productId)
    {
        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

        return product != null ? _mapper.Map<ProductCatalogDto>(product) : null;
    }

    public async Task<List<ProductCatalogDto>> GetProductsByCategoryAsync(string category, string organizationId)
    {
        var products = await _unitOfWork.Repository<ProductCatalog>()
            .FindAsync(p => p.Category == category && 
                           p.OrganizationId == organizationId && 
                           !p.IsDeleted);

        return _mapper.Map<List<ProductCatalogDto>>(products);
    }

    public async Task<List<ProductCatalogDto>> GetLowStockProductsAsync(string organizationId)
    {
        var products = await _unitOfWork.Repository<ProductCatalog>()
            .FindAsync(p => p.OrganizationId == organizationId && 
                           p.AvailableQuantity <= p.ReorderLevel && 
                           p.Status == ProductStatus.Active && 
                           !p.IsDeleted);

        return _mapper.Map<List<ProductCatalogDto>>(products);
    }

    public async Task<List<ProductCatalogDto>> SearchProductsAsync(string searchTerm, string organizationId)
    {
        var products = await _unitOfWork.Repository<ProductCatalog>()
            .FindAsync(p => p.OrganizationId == organizationId && 
                           (p.Name.Contains(searchTerm) || 
                            p.Description != null && p.Description.Contains(searchTerm) ||
                            p.Category.Contains(searchTerm)) &&
                           !p.IsDeleted);

        return _mapper.Map<List<ProductCatalogDto>>(products);
    }

    public async Task<bool> IsProductAvailableAsync(string productId, decimal requiredQuantity)
    {
        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId && 
                                     p.Status == ProductStatus.Active && 
                                     !p.IsDeleted);

        return product != null && product.AvailableQuantity >= requiredQuantity;
    }

    public async Task<List<ProductCatalogDto>> GetProductsBySupplierAsync(string supplierId, string organizationId)
    {
        var products = await _unitOfWork.Repository<ProductCatalog>()
            .FindAsync(p => p.OrganizationId == organizationId && 
                           p.SupplierIds != null && p.SupplierIds.Contains(supplierId) &&
                           !p.IsDeleted);

        return _mapper.Map<List<ProductCatalogDto>>(products);
    }

    public async Task RefreshProductCatalogAsync(string organizationId)
    {
        var masterProducts = await _masterDataService.GetProductsFromMasterDataAsync(organizationId);
        
        foreach (var masterProduct in masterProducts)
        {
            await SyncProductAsync(masterProduct.ProductId, organizationId);
        }
    }

    public async Task<ProductCatalogDto> UpdateProductAsync(string productId, ProductCatalogDto productData)
    {
        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found");
        }

        // Update the product with the new data
        _mapper.Map(productData, product);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<ProductCatalog>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductCatalogDto>(product);
    }

    public async Task<bool> DeleteProductAsync(string productId)
    {
        var product = await _unitOfWork.Repository<ProductCatalog>()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            return false;
        }

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<ProductCatalog>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task UpdateProductPricingAsync(ProductCatalog product)
    {
        // Get latest pricing from master data
        var pricing = await _masterDataService.GetProductPricingFromMasterDataAsync(product.ProductId, DateTime.UtcNow);
        if (pricing != null)
        {
            product.CurrentPrice = pricing.CurrentPrice;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task ValidateProductComplianceAsync(ProductCatalog product)
    {
        // Validate product compliance with master data
        var isValid = await _masterDataService.ValidateProductWithMasterDataAsync(product.ProductId);
        if (!isValid)
        {
            // Log compliance validation failure
            // This could trigger alerts or notifications
        }
    }
}