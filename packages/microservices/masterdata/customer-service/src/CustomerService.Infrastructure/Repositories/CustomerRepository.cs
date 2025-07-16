using Microsoft.EntityFrameworkCore;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;
using CustomerService.Infrastructure.Data;

namespace CustomerService.Infrastructure.Repositories;

public class CustomerRepository : Repository<CustomerService.Core.Entities.Customer>, ICustomerRepository
{
    public CustomerRepository(CustomerServiceDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<CustomerService.Core.Entities.Customer?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<CustomerService.Core.Entities.Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.ContactEmail.ToLower() == email.ToLower() && !e.IsDeleted);
    }

    public async Task<CustomerService.Core.Entities.Customer?> GetByTaxNumberAsync(string taxNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.TaxNumber == taxNumber && !e.IsDeleted);
    }

    public async Task<CustomerService.Core.Entities.Customer?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.RegistrationNumber == registrationNumber && !e.IsDeleted);
    }
}