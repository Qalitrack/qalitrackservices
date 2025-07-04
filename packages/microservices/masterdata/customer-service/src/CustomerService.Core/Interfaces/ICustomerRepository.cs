using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByTaxNumberAsync(string taxNumber);
    Task<Customer?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetWithContactsAsync(string id);
    Task<Customer?> GetWithContractsAsync(string id);
    Task<Customer?> GetWithBillingAsync(string id);
    Task<Customer?> GetWithCreditAsync(string id);
    Task<Customer?> GetWithAllDetailsAsync(string id);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
    Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType customerType);
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
}