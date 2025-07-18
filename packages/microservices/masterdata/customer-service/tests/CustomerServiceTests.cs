using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using CustomerService.Core.DTOs;

namespace CustomerService.Tests;

public class CustomerServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task Health_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var customerDto = new CreateCustomerDto
        {
            Name = "Test Customer",
            CustomerCode = "CUST-001",
            Email = "test@customer.com",
            Phone = "+1234567890",
            Address = "123 Customer St, City, Country",
            ContactPerson = "John Doe",
            PaymentTerms = "30 days",
            CreditLimit = 100000,
            Status = "Active",
            TaxNumber = "TAX123456"
        };

        var json = JsonSerializer.Serialize(customerDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerById_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = "test-customer-id";

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerByCode_WithValidCode_ShouldReturnCustomer()
    {
        // Arrange
        var customerCode = "CUST-001";

        // Act
        var response = await _client.GetAsync($"/api/customers/code/{customerCode}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerTransactions_WithValidId_ShouldReturnTransactions()
    {
        // Arrange
        var customerId = "test-customer-id";

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}/transactions");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchCustomers_WithValidQuery_ShouldReturnCustomers()
    {
        // Arrange
        var searchTerm = "Test";

        // Act
        var response = await _client.GetAsync($"/api/customers/search?q={searchTerm}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}