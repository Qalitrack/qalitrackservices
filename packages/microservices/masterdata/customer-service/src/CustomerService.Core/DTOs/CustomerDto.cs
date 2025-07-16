namespace CustomerService.Core.DTOs;

public class CustomerReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? TransporterId { get; set; }
    public string? PreferredTransporterId { get; set; }
    public bool IsSupplier { get; set; }
    public bool IsBuyer { get; set; }
    public int PaymentTermsDays { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string CustomerType { get; set; } = "Corporate";
    public decimal CreditLimit { get; set; }
    public string? Notes { get; set; }
    public bool IsSupplier { get; set; } = false;
    public bool IsBuyer { get; set; } = true;
    public int PaymentTermsDays { get; set; } = 30;
    public string Currency { get; set; } = "KES";
}

public class UpdateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public string? Notes { get; set; }
    public bool IsSupplier { get; set; }
    public bool IsBuyer { get; set; }
    public int PaymentTermsDays { get; set; }
    public string Currency { get; set; } = string.Empty;
}

// Additional DTOs for dual-role support
public class EnableTransporterRoleDto
{
    public string TransporterId { get; set; } = string.Empty;
}

public class SetPreferredTransporterDto
{
    public string TransporterId { get; set; } = string.Empty;
}