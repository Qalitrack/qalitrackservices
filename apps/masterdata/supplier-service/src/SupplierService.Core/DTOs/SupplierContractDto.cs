using SupplierService.Core.Entities;

namespace SupplierService.Core.DTOs;

public class SupplierContractDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    public ContractStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public decimal? ContractValue { get; set; }
    public string? Currency { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public int? PaymentDays { get; set; }
    public string? Terms { get; set; }
    public string? Conditions { get; set; }
    public bool AutoRenewal { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSupplierContractRequest
{
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public decimal? ContractValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public PaymentTerms PaymentTerms { get; set; }
    public int? PaymentDays { get; set; }
    public string? Terms { get; set; }
    public string? Conditions { get; set; }
    public bool AutoRenewal { get; set; } = false;
    public int? RenewalPeriodMonths { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSupplierContractRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    public ContractStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public decimal? ContractValue { get; set; }
    public string? Currency { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public int? PaymentDays { get; set; }
    public string? Terms { get; set; }
    public string? Conditions { get; set; }
    public bool AutoRenewal { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public string? Notes { get; set; }
}