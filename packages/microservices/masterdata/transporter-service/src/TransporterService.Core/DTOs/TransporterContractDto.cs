using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterContractDto
{
    public string Id { get; set; } = string.Empty;
    public string TransporterId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public ContractType ContractType { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientContactPerson { get; set; }
    public string? ClientPhone { get; set; }
    public string? ClientEmail { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string? PaymentTerms { get; set; }
    public ContractStatus Status { get; set; }
    public string? ServiceDescription { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? PerformanceMetrics { get; set; }
    public string? PenaltyClause { get; set; }
    public bool AutoRenewal { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}