using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierFinancialRepository : IRepository<SupplierFinancial>
{
    Task<SupplierFinancial?> GetBySupplierIdAsync(string supplierId);
}