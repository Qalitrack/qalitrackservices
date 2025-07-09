using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;

namespace ProductService.Tests.Helpers;

public static class MockFactories
{
    public static Mock<IProductService> CreateMockProductService()
    {
        var mock = new Mock<IProductService>();
        
        // Setup default behaviors for common scenarios
        SetupGetAllProductsAsync(mock);
        SetupGetProductByIdAsync(mock);
        SetupRegisterProductAsync(mock);
        SetupUpdateProductAsync(mock);
        SetupDeleteProductAsync(mock);
        SetupGetProductsByCategoryAsync(mock);
        SetupGetHazardousProductsAsync(mock);
        SetupSearchProductsAsync(mock);
        SetupGetProductSpecificationsAsync(mock);
        SetupUpdateProductSpecificationsAsync(mock);
        SetupGetProductPricingAsync(mock);
        SetupUpdateProductPricingAsync(mock);
        SetupGetProductComplianceAsync(mock);
        SetupGetAllCategoriesAsync(mock);
        SetupGetRootCategoriesAsync(mock);
        SetupGetSubCategoriesAsync(mock);
        SetupCreateCategoryAsync(mock);
        
        return mock;
    }

    public static Mock<IProductRepository> CreateMockProductRepository()
    {
        var mock = new Mock<IProductRepository>();
        
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(TestDataFactory.CreateTestProductList());
            
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => 
            {
                var product = TestDataFactory.CreateTestProduct();
                product.Id = id;
                return product;
            });
            
        mock.Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product product) => 
            {
                product.Id = Guid.NewGuid().ToString();
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                return product;
            });
            
        mock.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product product) => 
            {
                product.UpdatedAt = DateTime.UtcNow;
                return product;
            });
            
        mock.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.GetByCategoryAsync(It.IsAny<string>()))
            .ReturnsAsync((string categoryId) => 
                TestDataFactory.CreateTestProductList().Where(p => p.CategoryId == categoryId).ToList());
                
        mock.Setup(x => x.GetHazardousProductsAsync())
            .ReturnsAsync(TestDataFactory.CreateTestProductList().Where(p => p.IsHazardous).ToList());
            
        mock.Setup(x => x.SearchProductsAsync(It.IsAny<string>()))
            .ReturnsAsync((string searchTerm) => 
                TestDataFactory.CreateTestProductList().Where(p => 
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList());
        
        return mock;
    }

    public static Mock<IProductCategoryRepository> CreateMockProductCategoryRepository()
    {
        var mock = new Mock<IProductCategoryRepository>();
        
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(TestDataFactory.CreateTestCategoryHierarchy());
            
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => 
            {
                var category = TestDataFactory.CreateTestProductCategory();
                category.Id = id;
                return category;
            });
            
        mock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(TestDataFactory.CreateTestCategoryHierarchy().Where(c => c.ParentCategoryId == null).ToList());
            
        mock.Setup(x => x.GetSubCategoriesAsync(It.IsAny<string>()))
            .ReturnsAsync((string parentId) => 
                TestDataFactory.CreateTestCategoryHierarchy().Where(c => c.ParentCategoryId == parentId).ToList());
                
        mock.Setup(x => x.AddAsync(It.IsAny<ProductCategory>()))
            .ReturnsAsync((ProductCategory category) => 
            {
                category.Id = Guid.NewGuid().ToString();
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                return category;
            });
        
        return mock;
    }

    public static Mock<IProductComplianceRepository> CreateMockProductComplianceRepository()
    {
        var mock = new Mock<IProductComplianceRepository>();
        
        mock.Setup(x => x.GetByProductIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string productId) => 
            {
                return new List<ProductCompliance>
                {
                    TestDataFactory.CreateTestProductCompliance(productId)
                };
            });
        
        return mock;
    }

    public static Mock<IRepository<ProductSpecification>> CreateMockProductSpecificationRepository()
    {
        var mock = new Mock<IRepository<ProductSpecification>>();
        
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<ProductSpecification>());
            
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => 
            {
                var spec = TestDataFactory.CreateTestProductSpecification("test-product-id");
                spec.Id = id;
                return spec;
            });
            
        mock.Setup(x => x.AddAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync((ProductSpecification spec) => 
            {
                spec.Id = Guid.NewGuid().ToString();
                spec.CreatedAt = DateTime.UtcNow;
                spec.UpdatedAt = DateTime.UtcNow;
                return spec;
            });
        
        return mock;
    }

    public static Mock<IRepository<ProductPricing>> CreateMockProductPricingRepository()
    {
        var mock = new Mock<IRepository<ProductPricing>>();
        
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<ProductPricing>());
            
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => 
            {
                var pricing = TestDataFactory.CreateTestProductPricing("test-product-id");
                pricing.Id = id;
                return pricing;
            });
            
        mock.Setup(x => x.AddAsync(It.IsAny<ProductPricing>()))
            .ReturnsAsync((ProductPricing pricing) => 
            {
                pricing.Id = Guid.NewGuid().ToString();
                pricing.CreatedAt = DateTime.UtcNow;
                pricing.UpdatedAt = DateTime.UtcNow;
                return pricing;
            });
        
        return mock;
    }

    public static Mock<IRepository<ProductHazmat>> CreateMockProductHazmatRepository()
    {
        var mock = new Mock<IRepository<ProductHazmat>>();
        
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<ProductHazmat>());
            
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new ProductHazmat
            {
                Id = id,
                ProductId = "test-product-id",
                HazmatClass = "Class 3",
                UnNumber = "UN1993",
                PackingGroup = "II",
                ProperShippingName = "Flammable liquid, n.o.s.",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
            });
        
        return mock;
    }

    public static Mock<IMapper> CreateMockMapper()
    {
        var mock = new Mock<IMapper>();
        
        // Setup common mapping scenarios
        mock.Setup(x => x.Map<ProductDto>(It.IsAny<Product>()))
            .Returns((Product source) => 
            {
                if (source == null) return null;
                return TestDataFactory.CreateTestProductDto(source.Status);
            });
            
        mock.Setup(x => x.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
            .Returns((List<Product> source) => 
            {
                if (source == null) return new List<ProductDto>();
                return source.Select(p => TestDataFactory.CreateTestProductDto(p.Status)).ToList();
            });
            
        mock.Setup(x => x.Map<ProductCategoryDto>(It.IsAny<ProductCategory>()))
            .Returns((ProductCategory source) => 
            {
                if (source == null) return null;
                return TestDataFactory.CreateTestProductCategoryDto(source.ParentCategoryId);
            });
            
        mock.Setup(x => x.Map<List<ProductCategoryDto>>(It.IsAny<List<ProductCategory>>()))
            .Returns((List<ProductCategory> source) => 
            {
                if (source == null) return new List<ProductCategoryDto>();
                return source.Select(c => TestDataFactory.CreateTestProductCategoryDto(c.ParentCategoryId)).ToList();
            });
            
        mock.Setup(x => x.Map<Product>(It.IsAny<RegisterProductRequest>()))
            .Returns((RegisterProductRequest source) => 
            {
                if (source == null) return null;
                var product = TestDataFactory.CreateTestProduct();
                product.Name = source.Name;
                product.Code = source.Code;
                return product;
            });
            
        mock.Setup(x => x.Map<ProductCategory>(It.IsAny<CreateProductCategoryRequest>()))
            .Returns((CreateProductCategoryRequest source) => 
            {
                if (source == null) return null;
                var category = TestDataFactory.CreateTestProductCategory(source.ParentCategoryId);
                category.Name = source.Name;
                return category;
            });
        
        return mock;
    }

    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    // Helper methods for setting up specific behaviors
    private static void SetupGetAllProductsAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetAllProductsAsync())
            .ReturnsAsync(TestDataFactory.CreateTestProductList().Select(p => TestDataFactory.CreateTestProductDto(p.Status)).ToList());
    }

    private static void SetupGetProductByIdAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetProductByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => 
            {
                var dto = TestDataFactory.CreateTestProductDto();
                dto.Id = id;
                return dto;
            });
    }

    private static void SetupRegisterProductAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.RegisterProductAsync(It.IsAny<RegisterProductRequest>()))
            .ReturnsAsync((RegisterProductRequest request) => 
            {
                var dto = TestDataFactory.CreateTestProductDto();
                dto.Name = request.Name;
                dto.Code = request.Code;
                return dto;
            });
    }

    private static void SetupUpdateProductAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.UpdateProductAsync(It.IsAny<string>(), It.IsAny<UpdateProductRequest>()))
            .ReturnsAsync((string id, UpdateProductRequest request) => 
            {
                var dto = TestDataFactory.CreateTestProductDto();
                dto.Id = id;
                dto.Name = request.Name;
                return dto;
            });
    }

    private static void SetupDeleteProductAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.DeleteProductAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private static void SetupGetProductsByCategoryAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetProductsByCategoryAsync(It.IsAny<string>()))
            .ReturnsAsync((string categoryId) => 
                TestDataFactory.CreateTestProductList().Select(p => TestDataFactory.CreateTestProductDto(p.Status)).ToList());
    }

    private static void SetupGetHazardousProductsAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetHazardousProductsAsync())
            .ReturnsAsync(TestDataFactory.CreateTestProductList().Where(p => p.IsHazardous)
                .Select(p => TestDataFactory.CreateTestProductDto(p.Status)).ToList());
    }

    private static void SetupSearchProductsAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.SearchProductsAsync(It.IsAny<string>()))
            .ReturnsAsync((string searchTerm) => 
                TestDataFactory.CreateTestProductList().Select(p => TestDataFactory.CreateTestProductDto(p.Status)).ToList());
    }

    private static void SetupGetProductSpecificationsAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetProductSpecificationsAsync(It.IsAny<string>()))
            .ReturnsAsync((string productId) => new List<ProductSpecificationDto>
            {
                TestDataFactory.CreateTestProductSpecificationDto(productId)
            });
    }

    private static void SetupUpdateProductSpecificationsAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.UpdateProductSpecificationsAsync(It.IsAny<string>(), It.IsAny<UpdateProductSpecificationsRequest>()))
            .Returns(Task.CompletedTask);
    }

    private static void SetupGetProductPricingAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetProductPricingAsync(It.IsAny<string>()))
            .ReturnsAsync((string productId) => new List<ProductPricingDto>
            {
                TestDataFactory.CreateTestProductPricingDto(productId)
            });
    }

    private static void SetupUpdateProductPricingAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.UpdateProductPricingAsync(It.IsAny<string>(), It.IsAny<UpdateProductPricingRequest>()))
            .Returns(Task.CompletedTask);
    }

    private static void SetupGetProductComplianceAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetProductComplianceAsync(It.IsAny<string>()))
            .ReturnsAsync((string productId) => TestDataFactory.CreateTestProductComplianceDto(productId));
    }

    private static void SetupGetAllCategoriesAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(TestDataFactory.CreateTestCategoryHierarchy().Select(c => 
                TestDataFactory.CreateTestProductCategoryDto(c.ParentCategoryId)).ToList());
    }

    private static void SetupGetRootCategoriesAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(TestDataFactory.CreateTestCategoryHierarchy().Where(c => c.ParentCategoryId == null)
                .Select(c => TestDataFactory.CreateTestProductCategoryDto(c.ParentCategoryId)).ToList());
    }

    private static void SetupGetSubCategoriesAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.GetSubCategoriesAsync(It.IsAny<string>()))
            .ReturnsAsync((string parentId) => 
                TestDataFactory.CreateTestCategoryHierarchy().Where(c => c.ParentCategoryId == parentId)
                    .Select(c => TestDataFactory.CreateTestProductCategoryDto(c.ParentCategoryId)).ToList());
    }

    private static void SetupCreateCategoryAsync(Mock<IProductService> mock)
    {
        mock.Setup(x => x.CreateCategoryAsync(It.IsAny<CreateProductCategoryRequest>()))
            .ReturnsAsync((CreateProductCategoryRequest request) => 
            {
                var dto = TestDataFactory.CreateTestProductCategoryDto(request.ParentCategoryId);
                dto.Name = request.Name;
                return dto;
            });
    }
}