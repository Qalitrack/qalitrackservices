using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterPerformanceRepository : IRepository<TransporterPerformance>
{
    Task<IEnumerable<TransporterPerformance>> GetPerformanceByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterPerformance>> GetPerformanceByTypeAsync(string transporterId, PerformanceMetricType metricType);
    Task<IEnumerable<TransporterPerformance>> GetPerformanceByPeriodAsync(string transporterId, string period);
    Task<IEnumerable<TransporterPerformance>> GetPerformanceByDateRangeAsync(string transporterId, DateTime startDate, DateTime endDate);
    Task<TransporterPerformance?> GetLatestPerformanceAsync(string transporterId, PerformanceMetricType metricType);
    Task<decimal> GetAveragePerformanceAsync(string transporterId, PerformanceMetricType metricType, DateTime? startDate = null, DateTime? endDate = null);
}