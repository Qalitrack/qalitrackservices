using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierContactRepository : IRepository<SupplierContact>
{
    Task<IEnumerable<SupplierContact>> GetBySupplierIdAsync(string supplierId);
    Task<SupplierContact?> GetPrimaryContactAsync(string supplierId);
    Task<IEnumerable<SupplierContact>> GetByContactTypeAsync(string supplierId, ContactType contactType);
    Task<SupplierContact?> GetByEmailAsync(string email);
}