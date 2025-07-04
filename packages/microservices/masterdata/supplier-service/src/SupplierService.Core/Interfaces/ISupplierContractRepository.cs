using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierContractRepository : IRepository<SupplierContract>
{
    Task<IEnumerable<SupplierContract>> GetBySupplierIdAsync(string supplierId);
    Task<SupplierContract?> GetByContractNumberAsync(string contractNumber);
    Task<IEnumerable<SupplierContract>> GetByStatusAsync(ContractStatus status);
    Task<IEnumerable<SupplierContract>> GetExpiringContractsAsync(DateTime date);
    Task<IEnumerable<SupplierContract>> GetActiveContractsAsync(string supplierId);
}