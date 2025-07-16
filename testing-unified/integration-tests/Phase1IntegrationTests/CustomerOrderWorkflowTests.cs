using CustomerService.Core.DTOs;
using ProductService.Core.DTOs;
using SupplierService.Core.DTOs;
using CustomerService.Core.Entities;

namespace Phase1IntegrationTests;

/// <summary>
/// End-to-end tests for customer order creation workflow
/// Tests Requirements: 8.1, 8.2, 8.3
/// </summary>
public class CustomerOrderWorkflowTests : IClassFixture<Phase1TestFixture>
{
    private readonly Phase1TestFixture _fixture;
    private readonly HttpClient _customerClient;
    private readonly HttpClient _productClient;
    private readonly HttpClient _supplierClient;

    public CustomerOrderWorkflowTests(Phase1TestFixture fixture)
    {
        _fixture = fixture;
        _customerClient = _fixture.CustomerClient;
        _productClient = _fixture.ProductClient;
        _supplierClient = _fixture.SupplierClient;
    }

    [Fact]
    public async Task CreateOrder_WithValidCustomerAndProduct_ShouldSucceed()
    {
        // Arrange - Create a customer
        var createCustomerRequest = new CreateCustomerDto
        {
            Name = "Test Customer Ltd",
            Email = "test@customer.com",
            Phone = "+254700000001",
            Address = "123 Test Street, Nairobi",
            BusinessRegistrationNumber = "BRN001",
            TaxNumber = "TAX001",
            Currency = "KES",
            PaymentTermsDays = 30,
            CreditLimit = 100000,
            IsBuyer = true,
            IsSupplier = false
        };

        var customerResponse = await _customerClient.PostAsJsonAsync("/api/customers", createCustomerRequest);
        customerResponse.EnsureSuccessStatusCode();
        var customerContent = await customerResponse.Content.ReadAsStringAsync();
        var customer = JsonConvert.DeserializeObject<CustomerDto>(customerContent);

        // Arrange - Create a product
        var createProductRequest = new RegisterProductRequest
        {
            Name = "Test Product",
            Description = "Test product for order workflow",
            Category = "General",
            UnitOfMeasure = "KG",
            BasePrice = 1000,
            Currency = "KES",
            IsHazardous = false,
            RequiresSpecialHandling = false
        };

        var productResponse = await _productClient.PostAsJsonAsync("/api/products", createProductRequest);
        productResponse.EnsureSuccessStatusCode();
        var productContent = await productResponse.Content.ReadAsStringAsync();
        var productApiResponse = JsonConvert.DeserializeObject<ApiResponseDto<ProductDto>>(productContent);
        var product = productApiResponse!.Data;

        // Act - Create an order
        var createOrderRequest = new CreateOrderDto
        {
            CustomerId = customer!.Id,
            ProductId = product!.Id,
            Quantity = 100,
            UnitPrice = 1000,
            Currency = "KES",
            OrderType = OrderType.Purchase,
            DeliveryAddress = "456 Delivery Street, Nairobi",
            RequestedDeliveryDate = DateTime.UtcNow.AddDays(7),
            SpecialInstructions = "Handle with care"
        };

        var orderResponse = await _customerClient.PostAsJsonAsync("/api/orders", createOrderRequest);

        // Assert
        orderResponse.EnsureSuccessStatusCode();
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonConvert.DeserializeObject<OrderDto>(orderContent);

        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(customer.Id);
        order.ProductId.Should().Be(product.Id);
        order.Quantity.Should().Be(100);
        order.UnitPrice.Should().Be(1000);
        order.TotalAmount.Should().Be(100000); // 100 * 1000
        order.Status.Should().Be(OrderStatus.Pending);
        order.Currency.Should().Be("KES");
    }

    [Fact]
    public async Task CreateOrder_WithInvalidCustomer_ShouldFail()
    {
        // Arrange - Create a product first
        var createProductRequest = new RegisterProductRequest
        {
            Name = "Test Product",
            Description = "Test product for order workflow",
            Category = "General",
            UnitOfMeasure = "KG",
            BasePrice = 1000,
            Currency = "KES"
        };

        var productResponse = await _productClient.PostAsJsonAsync("/api/products", createProductRequest);
        productResponse.EnsureSuccessStatusCode();
        var productContent = await productResponse.Content.ReadAsStringAsync();
        var productApiResponse = JsonConvert.DeserializeObject<ApiResponseDto<ProductDto>>(productContent);
        var product = productApiResponse!.Data;

        // Act - Try to create order with invalid customer ID
        var createOrderRequest = new CreateOrderDto
        {
            CustomerId = "invalid-customer-id",
            ProductId = product!.Id,
            Quantity = 100,
            UnitPrice = 1000,
            Currency = "KES",
            OrderType = OrderType.Purchase
        };

        var orderResponse = await _customerClient.PostAsJsonAsync("/api/orders", createOrderRequest);

        // Assert
        orderResponse.IsSuccessStatusCode.Should().BeFalse();
        orderResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrderStatus_ThroughWorkflow_ShouldTrackHistory()
    {
        // Arrange - Create customer, product, and order
        var customer = await CreateTestCustomer("Workflow Customer");
        var product = await CreateTestProduct("Workflow Product");
        var order = await CreateTestOrder(customer.Id, product.Id);

        // Act - Update order status through workflow
        var updateStatusRequest = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Confirmed,
            Reason = "Order confirmed by customer",
            ChangedBy = "test-user"
        };

        var statusResponse = await _customerClient.PutAsJsonAsync($"/api/orders/{order.Id}/status", updateStatusRequest);
        statusResponse.EnsureSuccessStatusCode();

        // Get status history
        var historyResponse = await _customerClient.GetAsync($"/api/orders/{order.Id}/status-history");
        historyResponse.EnsureSuccessStatusCode();
        var historyContent = await historyResponse.Content.ReadAsStringAsync();
        var history = JsonConvert.DeserializeObject<List<OrderStatusHistoryDto>>(historyContent);

        // Assert
        history.Should().NotBeNull();
        history!.Should().HaveCountGreaterThan(0);
        history.Should().Contain(h => h.Status == OrderStatus.Confirmed);
        history.Should().Contain(h => h.Reason == "Order confirmed by customer");
        history.Should().Contain(h => h.ChangedBy == "test-user");
    }

    [Fact]
    public async Task CancelOrder_WithValidReason_ShouldUpdateStatus()
    {
        // Arrange
        var customer = await CreateTestCustomer("Cancel Customer");
        var product = await CreateTestProduct("Cancel Product");
        var order = await CreateTestOrder(customer.Id, product.Id);

        // Act
        var cancelRequest = new CancelOrderDto
        {
            CancellationReason = "Customer requested cancellation",
            CancelledBy = "test-user"
        };

        var cancelResponse = await _customerClient.PostAsJsonAsync($"/api/orders/{order.Id}/cancel", cancelRequest);
        cancelResponse.EnsureSuccessStatusCode();

        // Verify order is cancelled
        var orderResponse = await _customerClient.GetAsync($"/api/orders/{order.Id}");
        orderResponse.EnsureSuccessStatusCode();
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var updatedOrder = JsonConvert.DeserializeObject<OrderDto>(orderContent);

        // Assert
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
        updatedOrder.CancellationReason.Should().Be("Customer requested cancellation");
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

    private async Task<OrderDto> CreateTestOrder(string customerId, string productId)
    {
        var request = new CreateOrderDto
        {
            CustomerId = customerId,
            ProductId = productId,
            Quantity = 100,
            UnitPrice = 1000,
            Currency = "KES",
            OrderType = OrderType.Purchase
        };

        var response = await _customerClient.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<OrderDto>(content)!;
    }
}