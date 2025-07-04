using CustomerService.Core.Entities;

namespace CustomerService.Core.DTOs;

public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public decimal CreditLimit { get; set; }
    public CustomerStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CustomerDetailDto : CustomerDto
{
    public List<CustomerContactDto> Contacts { get; set; } = new();
    public List<CustomerContractDto> Contracts { get; set; } = new();
    public List<CustomerLocationDto> Locations { get; set; } = new();
    public CustomerBillingDto? Billing { get; set; }
    public CustomerCreditDto? Credit { get; set; }
    public CustomerPreferenceDto? Preferences { get; set; }
}

public class RegisterCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public decimal CreditLimit { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public decimal CreditLimit { get; set; }
    public CustomerStatus Status { get; set; }
    public string? Notes { get; set; }
}