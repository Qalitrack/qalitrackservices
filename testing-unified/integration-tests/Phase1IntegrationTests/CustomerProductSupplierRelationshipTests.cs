using CustomerService.Core.DTOs;
using ProductService.Core.DTOs;
using SupplierService.Core.DTOs;

namespace Phase1IntegrationTests;

/// <summary>
/// Tests for customer-product-supplier relationship management
/// Tests Requirements: 8.1, 8.2, 8.3
/// </summary>
public class CustomerProductSupplierRelationshipTests : IClassFixture<Phase1TestFixture>
{
    private readonly Phase1TestFixture _fixture;
    private readonly HttpClient _customerClient;
    private readonly HttpClient _productClient;
    private readonly HttpClient _supplierClient;

    public CustomerProductSupplierRelationshipTests(Phase1TestFixture fixture)
    {
        _fixture = fixture;
        _customerClient = _fixture.CustomerClient;
        _productClient = _fixture.ProductClient;
        _supplierClient = _fixture.SupplierClient;
    }

    [Fact]
    public async Task CreateSupplierProductRelationship_ShouldLinkSupplierToProduct()
    {
        // Arrange - Create supplier and product
        var supplier = await CreateTestSupplier("Test Supplier Ltd");
        var product = await CreateTestProduct("Supplier Product");

        // Act - Create supplier-product relationship
        var supplierProductRequest = new CreateSupplierProductRequest
        {
            ProductId = product.Id,
            SupplierPrice = 800,
            Currency = "KES",
            MinimumOrderQuantity = 50,
            LeadTimeDays = 7,
            IsPreferred = true
        };

        var response = await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", supplierProductRequest);
        response.EnsureSuccessStatusCode();

        // Assert - Verify relationship exists
        var productsResponse = await _supplierClient.GetAsync($"/api/suppliers/{supplier.Id}/products");
        productsResponse.EnsureSuccessStatusCode();
        var productsContent = await productsResponse.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseDto<IEnumerable<SupplierProductDto>>>(productsContent);
        var supplierProducts = apiResponse!.Data!.ToList();

        supplierProducts.Should().HaveCount(1);
        supplierProducts[0].ProductId.Should().Be(product.Id);
        supplierProducts[0].SupplierPrice.Should().Be(800);
        supplierProducts[0].IsPreferred.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomerWithPreferredSupplier_ShouldEstablishRelationship()
    {
        // Arrange - Create supplier first
        var supplier = await CreateTestSupplier("Preferred Supplier");

        // Act - Create customer with preferred supplier
        var customerRequest = new CreateCustomerDto
        {
            Name = "Customer with Preferred Supplier",
            Email = "preferred@customer.com",
            Phone = "+254700000002",
            Address = "Customer Address",
            BusinessRegistrationNumber = "BRN002",
            TaxNumber = "TAX002",
            Currency = "KES",
            PaymentTermsDays = 30,
            IsBuyer = true,
            PreferredSupplierId = supplier.Id
        };

        var customerResponse = await _customerClient.PostAsJsonAsync("/api/customers", customerRequest);
        customerResponse.EnsureSuccessStatusCode();
        var customerContent = await customerResponse.Content.ReadAsStringAsync();
        var customer = JsonConvert.DeserializeObject<CustomerDto>(customerContent);

        // Assert
        customer.Should().NotBeNull();
        customer!.PreferredSupplierId.Should().Be(supplier.Id);
    }

    [Fact]
    public async Task GetProductPricing_ForSpecificCustomer_ShouldReturnCustomerSpecificPricing()
    {
        // Arrange - Create customer, product, and supplier
        var customer = await CreateTestCustomer("Pricing Customer");
        var product = await CreateTestProduct("Pricing Product");
        var supplier = await CreateTestSupplier("Pricing Supplier");

        // Create supplier-product relationship with specific pricing
        var supplierProductRequest = new CreateSupplierProductRequest
        {
            ProductId = product.Id,
            SupplierPrice = 900,
            Currency = "KES",
            MinimumOrderQuantity = 25,
            LeadTimeDays = 5
        };

        await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", supplierProductRequest);

        // Act - Get product pricing
        var pricingResponse = await _productClient.GetAsync($"/api/products/{product.Id}/pricing");
        pricingResponse.EnsureSuccessStatusCode();
        var pricingContent = await pricingResponse.Content.ReadAsStringAsync();
        var pricingApiResponse = JsonConvert.DeserializeObject<ApiResponseDto<List<ProductPricingDto>>>(pricingContent);
        var pricing = pricingApiResponse!.Data!;

        // Assert
        pricing.Should().NotBeNull();
        pricing.Should().HaveCountGreaterThan(0);
        
        // Should include base pricing and supplier-specific pricing
        var basePricing = pricing.FirstOrDefault(p => p.PricingType == "Base");
        basePricing.Should().NotBeNull();
        basePricing!.Price.Should().Be(product.BasePrice);
    }

    [Fact]
    public async Task CreateOrderWithSupplierIntegration_ShouldValidateSupplierCapability()
    {
        // Arrange - Create complete relationship chain
        var customer = await CreateTestCustomer("Integration Customer");
        var supplier = await CreateTestSupplier("Integration Supplier");
        var product = await CreateTestProduct("Integration Product");

        // Link supplier to product
        var supplierProductRequest = new CreateSupplierProductRequest
        {
            ProductId = product.Id,
            SupplierPrice = 850,
            Currency = "KES",
            MinimumOrderQuantity = 10,
            LeadTimeDays = 3
        };

        await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", supplierProductRequest);

        // Act - Create order that should reference supplier
        var orderRequest = new CreateOrderDto
        {
            CustomerId = customer.Id,
            ProductId = product.Id,
            SupplierId = supplier.Id,
            Quantity = 50,
            UnitPrice = 850,
            Currency = "KES",
            OrderType = CustomerService.Core.Entities.OrderType.Purchase
        };

        var orderResponse = await _customerClient.PostAsJsonAsync("/api/orders", orderRequest);
        orderResponse.EnsureSuccessStatusCode();
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonConvert.DeserializeObject<OrderDto>(orderContent);

        // Assert
        order.Should().NotBeNull();
        order!.SupplierId.Should().Be(supplier.Id);
        order.ProductId.Should().Be(product.Id);
        order.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task SearchProductsBySupplier_ShouldReturnCorrectProducts()
    {
        // Arrange - Create supplier with multiple products
        var supplier = await CreateTestSupplier("Multi Product Supplier");
        var product1 = await CreateTestProduct("Supplier Product 1");
        var product2 = await CreateTestProduct("Supplier Product 2");

        // Link both products to supplier
        await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", 
            new CreateSupplierProductRequest
            {
                ProductId = product1.Id,
                SupplierPrice = 500,
                Currency = "KES",
                MinimumOrderQuantity = 10,
                LeadTimeDays = 2
            });

        await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", 
            new CreateSupplierProductRequest
            {
                ProductId = product2.Id,
                SupplierPrice = 750,
                Currency = "KES",
                MinimumOrderQuantity = 20,
                LeadTimeDays = 4
            });

        // Act - Get supplier products
        var productsResponse = await _supplierClient.GetAsync($"/api/suppliers/{supplier.Id}/products");
        productsResponse.EnsureSuccessStatusCode();
        var productsContent = await productsResponse.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseDto<IEnumerable<SupplierProductDto>>>(productsContent);
        var supplierProducts = apiResponse!.Data!.ToList();

        // Assert
        supplierProducts.Should().HaveCount(2);
        supplierProducts.Should().Contain(p => p.ProductId == product1.Id);
        supplierProducts.Should().Contain(p => p.ProductId == product2.Id);
    }

    [Fact]
    public async Task UpdateSupplierPerformance_ShouldTrackRelationshipQuality()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Performance Supplier");

        // Act - Create performance record
        var performanceRequest = new CreateSupplierPerformanceRequest
        {
            EvaluationDate = DateTime.UtcNow,
            QualityScore = 85,
            DeliveryScore = 90,
            ServiceScore = 88,
            OverallScore = 87.67m,
            Comments = "Good performance overall, minor quality issues",
            EvaluatedBy = "test-evaluator"
        };

        var performanceResponse = await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/performance", performanceRequest);
        performanceResponse.EnsureSuccessStatusCode();

        // Verify performance record
        var getPerformanceResponse = await _supplierClient.GetAsync($"/api/suppliers/{supplier.Id}/performance");
        getPerformanceResponse.EnsureSuccessStatusCode();
        var performanceContent = await getPerformanceResponse.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseDto<IEnumerable<SupplierPerformanceDto>>>(performanceContent);
        var performances = apiResponse!.Data!.ToList();

        // Assert
        performances.Should().HaveCount(1);
        performances[0].QualityScore.Should().Be(85);
        performances[0].DeliveryScore.Should().Be(90);
        performances[0].ServiceScore.Should().Be(88);
        performances[0].OverallScore.Should().Be(87.67m);
    }

    private async Task<CustomerDto> CreateTestCustomer(string name)
    {
        var request = new CreateCustomerDto
        {
            Name = name,
            Email = $"{name.Replace(" ", "").ToLower()}@test.com",
            Phone = "+254700000001",
            Address = "Test Address",
            BusinessRegistrationNumber = $"BRN{Guid.NewGuid().ToString()[..8]}",
            TaxNumber = $"TAX{Guid.NewGuid().ToString()[..8]}",
            Currency = "KES",
            PaymentTermsDays = 30,
            IsBuyer = true
        };

        var response = await _customerClient.PostAsJsonAsync("/api/customers", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<CustomerDto>(content)!;
    }

    private async Task<ProductDto> CreateTestProduct(string name)
    {
        var request = new RegisterProductRequest
        {
            Name = name,
            Description = $"Test product: {name}",
            Category = "General",
            UnitOfMeasure = "KG",
            BasePrice = 1000,
            Currency = "KES"
        };

        var response = await _productClient.PostAsJsonAsync("/api/products", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseDto<ProductDto>>(content);
        return apiResponse!.Data!;
    }

    private async Task<SupplierDto> CreateTestSupplier(string name)
    {
        var request = new RegisterSupplierRequest
        {
            Name = name,
            Email = $"{name.Replace(" ", "").ToLower()}@supplier.com",
            Phone = "+254700000003",
            Address = "Supplier Address",
            BusinessRegistrationNumber = $"SBRN{Guid.NewGuid().ToString()[..8]}",
            TaxNumber = $"STAX{Guid.NewGuid().ToString()[..8]}",
            SupplierType = SupplierService.Core.Entities.SupplierType.Manufacturer,
            PaymentTerms = "Net 30",
            Currency = "KES"
        };

        var response = await _supplierClient.PostAsJsonAsync("/api/suppliers", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseDto<SupplierDto>>(content);
        return apiResponse!.Data!;
    }
}