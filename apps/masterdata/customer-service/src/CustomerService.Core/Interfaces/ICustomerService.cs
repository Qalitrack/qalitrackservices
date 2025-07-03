using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerRequest request);
    Task<CustomerDto?> GetCustomerAsync(string id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto?> GetCustomerByTaxNumberAsync(string taxNumber);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<IEnumerable<CustomerDto>> GetCustomersPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
    Task<CustomerDto> UpdateCustomerAsync(string id, UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(string id);
    Task<bool> ActivateCustomerAsync(string id);
    Task<bool> DeactivateCustomerAsync(string id);
    Task<CustomerDetailDto?> GetCustomerDetailsAsync(string id);
    
    Task<CustomerContactDto> AddContactAsync(string customerId, CreateCustomerContactRequest request);
    Task<IEnumerable<CustomerContactDto>> GetCustomerContactsAsync(string customerId);
    Task<CustomerContactDto> UpdateContactAsync(string contactId, UpdateCustomerContactRequest request);
    Task<bool> DeleteContactAsync(string contactId);
    
    Task<CustomerContractDto> CreateContractAsync(string customerId, CreateCustomerContractRequest request);
    Task<IEnumerable<CustomerContractDto>> GetCustomerContractsAsync(string customerId);
    Task<CustomerContractDto> UpdateContractAsync(string contractId, UpdateCustomerContractRequest request);
    Task<bool> DeleteContractAsync(string contractId);
    
    Task<CustomerBillingDto> UpdateBillingAsync(string customerId, UpdateCustomerBillingRequest request);
    Task<CustomerBillingDto?> GetCustomerBillingAsync(string customerId);
    
    Task<CustomerCreditDto> UpdateCreditAsync(string customerId, UpdateCustomerCreditRequest request);
    Task<CustomerCreditDto?> GetCustomerCreditAsync(string customerId);
}