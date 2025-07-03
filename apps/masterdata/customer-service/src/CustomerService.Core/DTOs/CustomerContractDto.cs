using CustomerService.Core.Entities;

namespace CustomerService.Core.DTOs;

public class CustomerContractDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractStatus Status { get; set; }
    public ContractType ContractType { get; set; }
    public string? Terms { get; set; }
    public string? SignedByCustomer { get; set; }
    public string? SignedByCompany { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCustomerContractRequest
{
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractType ContractType { get; set; }
    public string? Terms { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
}

public class UpdateCustomerContractRequest
{
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractStatus Status { get; set; }
    public ContractType ContractType { get; set; }
    public string? Terms { get; set; }
    public string? SignedByCustomer { get; set; }
    public string? SignedByCompany { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
}