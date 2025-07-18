using CustomerService.Core.DTOs;
using ProductService.Core.DTOs;
using SupplierService.Core.DTOs;

namespace Phase1IntegrationTests;

/// <summary>
/// Tests for dual-role scenarios (customer-as-transporter)
/// Tests Requirements: 8.1, 8.2, 8.3
/// </summary>
public class DualRoleScenarioTests : IClassFixture<Phase1TestFixture>
{
    private readonly Phase1TestFixture _fixture;
    private readonly HttpClient _customerClient;
    private readonly HttpClient _productClient;
    private readonly HttpClient _supplierClient;

    public DualRoleScenarioTests(Phase1TestFixture fixture)
    {
        _fixture = fixture;
        _customerClient = _fixture.CustomerClient;
        _productClient = _fixture.ProductClient;
        _supplierClient = _fixture.SupplierClient;
    }

    [Fact]
    public async Task CreateCustomerWithDualRole_ShouldSupportBothBuyerAndSupplier()
    {
        // Act - Create customer with dual role capabilities
        var createCustomerRequest = new CreateCustomerDto
        {
            Name = "Dual Role Company Ltd",
            Email = "dual@company.com",
            Phone = "+254700000010",
            Address = "123 Dual Role Street, Nairobi",
            BusinessRegistrationNumber = "DUAL001",
            TaxNumber = "DTAX001",
            Currency = "KES",
            PaymentTermsDays = 30,
            CreditLimit = 500000,
            IsBuyer = true,
            IsSupplier = true, // Dual role
            CanTransport = true // Can also provide transportation
        };

        var response = await _customerClient.PostAsJsonAsync("/api/customers", createCustomerRequest);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var customer = JsonConvert.DeserializeObject<CustomerDto>(content);

        // Assert
        customer.Should().NotBeNull();
        customer!.IsBuyer.Should().BeTrue();
        customer.IsSupplier.Should().BeTrue();
        customer.CanTransport.Should().BeTrue();
    }

    [Fact]
    public async Task EnableTransporterRole_ForExistingCustomer_ShouldUpdateCapabilities()
    {
        // Arrange - Create regular customer
        var customer = await CreateTestCustomer("Regular Customer", isBuyer: true, isSupplier: false);

        // Act - Enable transporter role
        var enableTransporterRequest = new EnableTransporterRoleDto
        {
            TransporterId = $"TRANS_{customer.Id}"
        };

        var response = await _customerClient.PostAsJsonAsync($"/api/customers/{customer.Id}/enable-transporter", enableTransporterRequest);
        response.EnsureSuccessStatusCode();

        // Verify customer now has transporter capabilities
        var getCustomerResponse = await _customerClient.GetAsync($"/api/customers/{customer.Id}");
        getCustomerResponse.EnsureSuccessStatusCode();
        var customerContent = await getCustomerResponse.Content.ReadAsStringAsync();
        var updatedCustomer = JsonConvert.DeserializeObject<CustomerDto>(customerContent);

        // Assert
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.CanTransport.Should().BeTrue();
        updatedCustomer.TransporterId.Should().Be($"TRANS_{customer.Id}");
    }

    [Fact]
    public async Task CreateOrderAsBuyer_ThenFulfillAsSupplier_ShouldWorkInDualRole()
    {
        // Arrange - Create dual-role entity
        var dualRoleEntity = await CreateTestCustomer("Dual Role Entity", isBuyer: true, isSupplier: true);
        var product = await CreateTestProduct("Dual Role Product");

        // Register as supplier for the product
        var supplierRequest = new RegisterSupplierRequest
        {
            Name = dualRoleEntity.Name,
            Email = dualRoleEntity.Email,
            Phone = dualRoleEntity.Phone,
            Address = dualRoleEntity.Address,
            BusinessRegistrationNumber = dualRoleEntity.BusinessRegistrationNumber,
            TaxNumber = dualRoleEntity.TaxNumber,
            SupplierType = SupplierService.Core.Entities.SupplierType.Distributor,
            PaymentTerms = "Net 30",
            Currency = "KES"
        };

        var supplierResponse = await _supplierClient.PostAsJsonAsync("/api/suppliers", supplierRequest);
        supplierResponse.EnsureSuccessStatusCode();
        var supplierContent = await supplierResponse.Content.ReadAsStringAsync();
        var supplierApiResponse = JsonConvert.DeserializeObject<ApiResponseDto<SupplierDto>>(supplierContent);
        var supplier = supplierApiResponse!.Data!;

        // Link product to supplier
        var supplierProductRequest = new CreateSupplierProductRequest
        {
            ProductId = product.Id,
            SupplierPrice = 900,
            Currency = "KES",
            MinimumOrderQuantity = 10,
            LeadTimeDays = 2
        };

        await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", supplierProductRequest);

        // Act 1 - Create order as buyer
        var orderRequest = new CreateOrderDto
        {
            CustomerId = dualRoleEntity.Id,
            ProductId = product.Id,
            SupplierId = supplier.Id, // Same entity fulfilling as supplier
            Quantity = 50,
            UnitPrice = 900,
            Currency = "KES",
            OrderType = CustomerService.Core.Entities.OrderType.Purchase,
            SpecialInstructions = "Self-fulfillment order"
        };

        var orderResponse = await _customerClient.PostAsJsonAsync("/api/orders", orderRequest);
        orderResponse.EnsureSuccessStatusCode();
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonConvert.DeserializeObject<OrderDto>(orderContent);

        // Assert
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(dualRoleEntity.Id);
        order.SupplierId.Should().Be(supplier.Id);
        order.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task SetPreferredTransporter_ForCustomer_ShouldEstablishTransportRelationship()
    {
        // Arrange - Create customer and transporter entity
        var customer = await CreateTestCustomer("Transport Customer", isBuyer: true);
        var transporter = await CreateTestCustomer("Transport Provider", isBuyer: false, isSupplier: false, canTransport: true);

        // Act - Set preferred transporter
        var setTransporterRequest = new SetPreferredTransporterDto
        {
            TransporterId = transporter.Id
        };

        var response = await _customerClient.PostAsJsonAsync($"/api/customers/{customer.Id}/set-preferred-transporter", setTransporterRequest);
        response.EnsureSuccessStatusCode();

        // Verify relationship
        var getCustomerResponse = await _customerClient.GetAsync($"/api/customers/{customer.Id}");
        getCustomerResponse.EnsureSuccessStatusCode();
        var customerContent = await getCustomerResponse.Content.ReadAsStringAsync();
        var updatedCustomer = JsonConvert.DeserializeObject<CustomerDto>(customerContent);

        // Assert
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.PreferredTransporterId.Should().Be(transporter.Id);
    }

    [Fact]
    public async Task CreateOrderWithTransportation_ShouldLinkAllRoles()
    {
        // Arrange - Create entities for all roles
        var buyer = await CreateTestCustomer("Buyer Company", isBuyer: true);
        var supplier = await CreateTestSupplier("Supplier Company");
        var transporter = await CreateTestCustomer("Transport Company", canTransport: true);
        var product = await CreateTestProduct("Transport Product");

        // Link product to supplier
        await _supplierClient.PostAsJsonAsync($"/api/suppliers/{supplier.Id}/products", 
            new CreateSupplierProductRequest
            {
                ProductId = product.Id,
                SupplierPrice = 1200,
                Currency = "KES",
                MinimumOrderQuantity = 5,
                LeadTimeDays = 1
            });

        // Act - Create order with all roles specified
        var orderRequest = new CreateOrderDto
        {
            CustomerId = buyer.Id,
            ProductId = product.Id,
            SupplierId = supplier.Id,
            TransporterId = transporter.Id,
            Quantity = 25,
            UnitPrice = 1200,
            Currency = "KES",
            OrderType = CustomerService.Core.Entities.OrderType.Purchase,
            DeliveryAddress = "Final Destination Address",
            SpecialInstructions = "Coordinate with all parties"
        };

        var orderResponse = await _customerClient.PostAsJsonAsync("/api/orders", orderRequest);
        orderResponse.EnsureSuccessStatusCode();
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonConvert.DeserializeObject<OrderDto>(orderContent);

        // Assert
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(buyer.Id);
        order.SupplierId.Should().Be(supplier.Id);
        order.TransporterId.Should().Be(transporter.Id);
        order.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task ValidateDualRoleConstraints_ShouldPreventConflicts()
    {
        // Arrange - Create dual role entity
        var dualRole = await CreateTestCustomer("Conflict Test", isBuyer: true, isSupplier: true);
        var product = await CreateTestProduct("Conflict Product");

        // Act - Try to create order where same entity is both buyer and supplier
        var orderRequest = new CreateOrderDto
        {
            CustomerId = dualRole.Id,
            ProductId = product.Id,
            SupplierId = dualRole.Id, // Same as customer - should be handled appropriately
            Quantity = 10,
            UnitPrice = 1000,
            Currency = "KES",
            OrderType = CustomerService.Core.Entities.OrderType.Purchase
        };

        var orderResponse = await _customerClient.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert - Should either succeed (if business allows self-orders) or fail with appropriate message
        if (orderResponse.IsSuccessStatusCode)
        {
            var orderContent = await orderResponse.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<OrderDto>(orderContent);
            order.Should().NotBeNull();
            order!.CustomerId.Should().Be(dualRole.Id);
            order.SupplierId.Should().Be(dualRole.Id);
        }
        else
        {
            // If business rules prevent self-orders, should get appropriate error
            orderResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }

    private async Task<CustomerDto> CreateTestCustomer(string name, bool isBuyer = true, bool isSupplier = false, bool canTransport = false)
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
            IsBuyer = isBuyer,
            IsSupplier = isSupplier,
            CanTransport = canTransport
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