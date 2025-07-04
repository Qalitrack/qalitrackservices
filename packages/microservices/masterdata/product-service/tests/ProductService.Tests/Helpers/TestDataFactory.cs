using ProductService.Core.DTOs;
using ProductService.Core.Entities;

namespace ProductService.Tests.Helpers;

public static class TestDataFactory
{
    // Product Test Data
    public static Product CreateTestProduct(ProductStatus status = ProductStatus.Active, bool isHazardous = false)
    {
        return new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Product",
            Code = GenerateProductCode(),
            Description = "Test product for unit testing",
            CategoryId = Guid.NewGuid().ToString(),
            UnitOfMeasure = "KG",
            Weight = 100.5m,
            Density = 0.8m,
            IsHazardous = isHazardous,
            HazmatClass = isHazardous ? "Class 3" : null,
            RequiresSpecialHandling = isHazardous,
            Status = status,
            Notes = "Test notes",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static List<Product> CreateTestProductList(int count = 5)
    {
        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            var product = CreateTestProduct();
            product.Code = $"TST{i + 1:D3}";
            product.Name = $"Test Product {i + 1}";
            product.IsHazardous = i % 2 == 0; // Every other product is hazardous
            products.Add(product);
        }
        return products;
    }

    public static ProductDto CreateTestProductDto(ProductStatus status = ProductStatus.Active)
    {
        return new ProductDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Product DTO",
            Code = GenerateProductCode(),
            Description = "Test product DTO for unit testing",
            CategoryId = Guid.NewGuid().ToString(),
            CategoryName = "Test Category",
            UnitOfMeasure = "KG",
            Weight = 100.5m,
            Density = 0.8m,
            IsHazardous = false,
            HazmatClass = null,
            RequiresSpecialHandling = false,
            Status = status.ToString(),
            Notes = "Test notes",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static RegisterProductRequest CreateValidRegisterProductRequest()
    {
        return new RegisterProductRequest
        {
            Name = "New Test Product",
            Code = GenerateProductCode(),
            Description = "New test product for registration",
            CategoryId = Guid.NewGuid().ToString(),
            UnitOfMeasure = "LTR",
            Weight = 50.0m,
            Density = 1.2m,
            IsHazardous = false,
            HazmatClass = null,
            RequiresSpecialHandling = false,
            Notes = "Registration test notes"
        };
    }

    public static RegisterProductRequest CreateHazardousProductRequest()
    {
        return new RegisterProductRequest
        {
            Name = "Hazardous Test Product",
            Code = GenerateProductCode(),
            Description = "Hazardous test product for registration",
            CategoryId = Guid.NewGuid().ToString(),
            UnitOfMeasure = "KG",
            Weight = 75.0m,
            Density = 0.9m,
            IsHazardous = true,
            HazmatClass = "Class 3",
            RequiresSpecialHandling = true,
            Notes = "Hazardous material test"
        };
    }

    public static UpdateProductRequest CreateValidUpdateProductRequest()
    {
        return new UpdateProductRequest
        {
            Name = "Updated Test Product",
            Description = "Updated test product description",
            CategoryId = Guid.NewGuid().ToString(),
            UnitOfMeasure = "TON",
            Weight = 200.0m,
            Density = 1.5m,
            IsHazardous = false,
            HazmatClass = null,
            RequiresSpecialHandling = false,
            Status = ProductStatus.Active.ToString(),
            Notes = "Updated test notes"
        };
    }

    // Product Category Test Data
    public static ProductCategory CreateTestProductCategory(string? parentId = null, bool isRoot = true)
    {
        return new ProductCategory
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Category",
            Description = "Test category for unit testing",
            ParentCategoryId = parentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static List<ProductCategory> CreateTestCategoryHierarchy()
    {
        var rootCategory = CreateTestProductCategory();
        rootCategory.Name = "Root Category";
        
        var childCategory1 = CreateTestProductCategory(rootCategory.Id, false);
        childCategory1.Name = "Child Category 1";
        
        var childCategory2 = CreateTestProductCategory(rootCategory.Id, false);
        childCategory2.Name = "Child Category 2";

        return new List<ProductCategory> { rootCategory, childCategory1, childCategory2 };
    }

    public static ProductCategoryDto CreateTestProductCategoryDto(string? parentId = null)
    {
        return new ProductCategoryDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Category DTO",
            Description = "Test category DTO for unit testing",
            ParentCategoryId = parentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static CreateProductCategoryRequest CreateValidCategoryRequest(string? parentId = null)
    {
        return new CreateProductCategoryRequest
        {
            Name = "New Test Category",
            Code = $"CAT{DateTime.UtcNow.Ticks % 10000:D4}",
            Description = "New test category for creation",
            ParentCategoryId = parentId
        };
    }

    // Product Specification Test Data
    public static ProductSpecification CreateTestProductSpecification(string productId)
    {
        return new ProductSpecification
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            Name = "Test Specification",
            Value = "Test Value",
            Unit = "Unit",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static ProductSpecificationDto CreateTestProductSpecificationDto(string productId)
    {
        return new ProductSpecificationDto
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            Name = "Test Specification DTO",
            Value = "Test Value DTO",
            Unit = "Unit"
        };
    }

    public static UpdateProductSpecificationsRequest CreateValidSpecificationsRequest()
    {
        return new UpdateProductSpecificationsRequest
        {
            Specifications = new List<ProductSpecificationDto>
            {
                new()
                {
                    Name = "Viscosity",
                    Value = "High",
                    Unit = "cP"
                },
                new()
                {
                    Name = "Flash Point",
                    Value = "60",
                    Unit = "°C"
                }
            }
        };
    }

    // Product Pricing Test Data
    public static ProductPricing CreateTestProductPricing(string productId)
    {
        return new ProductPricing
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            PricingType = PricingType.Standard,
            UnitPrice = 99.99m,
            Currency = "USD",
            EffectiveDate = DateTime.UtcNow.AddDays(-30),
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static ProductPricingDto CreateTestProductPricingDto(string productId)
    {
        return new ProductPricingDto
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            PricingType = "Standard",
            UnitPrice = 149.99m,
            Currency = "USD",
            EffectiveDate = DateTime.UtcNow.AddDays(-15),
            ExpiryDate = DateTime.UtcNow.AddDays(45)
        };
    }

    public static UpdateProductPricingRequest CreateValidPricingRequest()
    {
        return new UpdateProductPricingRequest
        {
            PricingRules = new List<ProductPricingDto>
            {
                new()
                {
                    PricingType = "Standard",
                    UnitPrice = 199.99m,
                    Currency = "USD",
                    EffectiveDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6)
                }
            }
        };
    }

    // Product Compliance Test Data
    public static ProductCompliance CreateTestProductCompliance(string productId)
    {
        return new ProductCompliance
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            ComplianceType = "DOT",
            Regulation = "Transportation Regulation",
            Authority = "DOT",
            CertificationNumber = "UN1993",
            Status = ComplianceStatus.Active,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static ProductComplianceDto CreateTestProductComplianceDto(string productId)
    {
        return new ProductComplianceDto
        {
            ProductId = productId,
            IsHazardous = false,
            HazmatClass = null,
            TransportRequirements = "Standard transport",
            StorageRequirements = "Room temperature",
            Requirements = new List<ComplianceRequirementDto>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ComplianceType = "EPA",
                    Regulation = "Environmental Regulation",
                    Authority = "EPA",
                    CertificationNumber = "EPA-123",
                    Status = "Active"
                }
            }
        };
    }

    // Utility Methods
    public static string GenerateProductCode()
    {
        return $"PRD{DateTime.UtcNow.Ticks % 100000:D5}";
    }

    public static string GenerateUniqueName(string baseName)
    {
        return $"{baseName}_{DateTime.UtcNow.Ticks % 10000}";
    }

    // Invalid/Edge Case Test Data
    public static RegisterProductRequest CreateInvalidProductRequest()
    {
        return new RegisterProductRequest
        {
            Name = "", // Invalid: empty name
            Code = "", // Invalid: empty code
            CategoryId = "", // Invalid: empty category
            UnitOfMeasure = "" // Invalid: empty unit
        };
    }

    public static UpdateProductRequest CreateInvalidUpdateRequest()
    {
        return new UpdateProductRequest
        {
            Name = "", // Invalid: empty name
            CategoryId = "", // Invalid: empty category
            UnitOfMeasure = "", // Invalid: empty unit
            Status = "InvalidStatus" // Invalid: unknown status
        };
    }

    public static CreateProductCategoryRequest CreateInvalidCategoryRequest()
    {
        return new CreateProductCategoryRequest
        {
            Name = "", // Invalid: empty name
            ParentCategoryId = "invalid-guid" // Invalid: malformed GUID
        };
    }
}