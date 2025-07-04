using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierProductRepository : IRepository<SupplierProduct>
{
    Task<IEnumerable<SupplierProduct>> GetBySupplierIdAsync(string supplierId);
    Task<IEnumerable<SupplierProduct>> GetByProductIdAsync(string productId);
    Task<IEnumerable<SupplierProduct>> GetByCategoryAsync(string category);
    Task<IEnumerable<SupplierProduct>> GetActiveProductsAsync(string supplierId);
}