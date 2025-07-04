using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierPerformanceRepository : IRepository<SupplierPerformance>
{
    Task<IEnumerable<SupplierPerformance>> GetBySupplierIdAsync(string supplierId);
    Task<SupplierPerformance?> GetBySupplierAndPeriodAsync(string supplierId, int year, int month);
    Task<IEnumerable<SupplierPerformance>> GetByPeriodAsync(int year, int? month = null);
    Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null);
}