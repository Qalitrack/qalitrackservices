using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface ICustomerRepository : IRepository<CustomerService.Core.Entities.Customer>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<CustomerService.Core.Entities.Customer?> GetByNameAsync(string name);
    Task<CustomerService.Core.Entities.Customer?> GetByEmailAsync(string email);
    Task<CustomerService.Core.Entities.Customer?> GetByTaxNumberAsync(string taxNumber);
    Task<CustomerService.Core.Entities.Customer?> GetByRegistrationNumberAsync(string registrationNumber);
}