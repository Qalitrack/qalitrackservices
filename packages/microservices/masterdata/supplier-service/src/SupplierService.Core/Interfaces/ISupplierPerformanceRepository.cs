using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierPerformanceRepository : IRepository<SupplierPerformance>
{
    Task<IEnumerable<SupplierPerformance>> GetBySupplierIdAsync(string supplierId);
    Task<SupplierPerformance?> GetBySupplierAndPeriodAsync(string supplierId, int year, int month);
    Task<IEnumerable<SupplierPerformance>> GetByPeriodAsync(int year, int? month = null);
    Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null);
    Task<IEnumerable<SupplierPerformance>> GetBySupplierId(string supplierId);
    Task<IEnumerable<SupplierPerformance>> GetByMetricType(string supplierId, PerformanceMetricType metricType);
    Task<IEnumerable<SupplierPerformance>> GetByPeriod(string supplierId, PerformancePeriod period);
    Task<IEnumerable<SupplierPerformance>> GetByDateRange(string supplierId, DateTime startDate, DateTime endDate);
    Task<decimal> GetAverageScore(string supplierId, PerformanceMetricType metricType);
    Task<SupplierPerformance?> GetLatestPerformanceAsync(string supplierId);
    Task<IEnumerable<Supplier>> GetTopPerformersAsync(int count = 10);
    Task<IEnumerable<Supplier>> GetPoorPerformersAsync(int count = 10);
}