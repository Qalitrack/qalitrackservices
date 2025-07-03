using Microsoft.EntityFrameworkCore;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;
using CustomerService.Infrastructure.Data;

namespace CustomerService.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(CustomerDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByTaxNumberAsync(string taxNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.TaxNumber == taxNumber && !c.IsDeleted);
    }

    public async Task<Customer?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber && !c.IsDeleted);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ContactEmail == email && !c.IsDeleted);
    }

    public async Task<Customer?> GetWithContactsAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Contacts.Where(ct => !ct.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Customer?> GetWithContractsAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Contracts.Where(ct => !ct.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Customer?> GetWithBillingAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Billing)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Customer?> GetWithCreditAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Credit)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Customer?> GetWithAllDetailsAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Contacts.Where(ct => !ct.IsDeleted))
            .Include(c => c.Contracts.Where(ct => !ct.IsDeleted))
            .Include(c => c.Locations.Where(l => !l.IsDeleted))
            .Include(c => c.Billing)
            .Include(c => c.Credit)
            .Include(c => c.Preferences)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType customerType)
    {
        return await _dbSet
            .Where(c => c.CustomerType == customerType && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(c => !c.IsDeleted && (
                c.Name.ToLower().Contains(term) ||
                (c.TaxNumber != null && c.TaxNumber.ToLower().Contains(term)) ||
                (c.RegistrationNumber != null && c.RegistrationNumber.ToLower().Contains(term)) ||
                c.ContactEmail.ToLower().Contains(term) ||
                (c.ContactPhone != null && c.ContactPhone.Contains(term))
            ))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}