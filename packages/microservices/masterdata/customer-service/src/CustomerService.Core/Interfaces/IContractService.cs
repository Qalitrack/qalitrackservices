using CustomerService.Core.DTOs;

namespace CustomerService.Core.Interfaces;

public interface IContractService
{
    Task<ContractReadDto> CreateContractAsync(CreateContractDto createContractDto);
    Task<ContractReadDto?> GetContractAsync(string id);
    Task<IEnumerable<ContractReadDto>> GetCustomerContractsAsync(string customerId);
    Task<ContractReadDto?> UpdateContractAsync(string id, UpdateContractDto updateContractDto);
    Task<bool> DeleteContractAsync(string id);
    Task<bool> ActivateContractAsync(string id);
    Task<bool> SuspendContractAsync(string id, string? reason = null);
    Task<bool> TerminateContractAsync(string id, string? reason = null);
    Task<ContractRenewalReadDto> RenewContractAsync(string contractId, RenewContractDto renewalDto);
    Task<IEnumerable<ContractReadDto>> GetExpiringContractsAsync(int daysAhead = 30);
    Task<IEnumerable<ContractReadDto>> GetContractsForRenewalNotificationAsync();
    Task<IEnumerable<ContractRenewalReadDto>> GetContractRenewalsAsync(string contractId);
    Task<IEnumerable<ContractReadDto>> GetActiveContractsAsync();
    Task<IEnumerable<ContractReadDto>> SearchContractsAsync(string searchTerm);
}