using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface IProcurementRepository : IRepository<Procurement>
{
    Task<IEnumerable<Procurement>> GetBySupplierIdAsync(string supplierId);
    Task<Procurement?> GetByProcurementNumberAsync(string procurementNumber);
    Task<IEnumerable<Procurement>> GetByStatusAsync(ProcurementStatus status);
    Task<IEnumerable<Procurement>> GetByPriorityAsync(ProcurementPriority priority);
    Task<IEnumerable<Procurement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Procurement>> GetPendingApprovalsAsync();
    Task<IEnumerable<Procurement>> GetOverdueDeliveriesAsync();
    Task<decimal> GetTotalValueBySupplierAsync(string supplierId, DateTime? startDate = null, DateTime? endDate = null);
}