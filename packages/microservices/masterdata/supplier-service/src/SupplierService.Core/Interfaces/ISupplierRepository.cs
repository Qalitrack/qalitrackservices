using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByNameAsync(string name);
    Task<Supplier?> GetByTaxNumberAsync(string taxNumber);
    Task<Supplier?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<IEnumerable<Supplier>> GetByStatusAsync(SupplierStatus status);
    Task<IEnumerable<Supplier>> GetByTypeAsync(SupplierType type);
    Task<IEnumerable<Supplier>> SearchAsync(string searchTerm);
    Task<Supplier?> GetWithContactsAsync(string id);
    Task<Supplier?> GetWithContractsAsync(string id);
    Task<Supplier?> GetWithProductsAsync(string id);
    Task<Supplier?> GetWithPerformanceAsync(string id);
    Task<Supplier?> GetWithFinancialAsync(string id);
    Task<Supplier?> GetCompleteAsync(string id);
}