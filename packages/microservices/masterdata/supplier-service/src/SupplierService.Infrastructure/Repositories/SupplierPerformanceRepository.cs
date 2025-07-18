using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierPerformanceRepository : Repository<SupplierPerformance>, ISupplierPerformanceRepository
{
    public SupplierPerformanceRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierPerformance>> GetBySupplierId(string supplierId)
    {
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && !sp.IsDeleted)
            .OrderByDescending(sp => sp.PeriodEnd)
            .ThenBy(sp => sp.MetricType)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPerformance>> GetByMetricType(string supplierId, PerformanceMetricType metricType)
    {
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && 
                        sp.MetricType == metricType && 
                        !sp.IsDeleted)
            .OrderByDescending(sp => sp.PeriodEnd)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPerformance>> GetByPeriod(string supplierId, PerformancePeriod period)
    {
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && 
                        sp.Period == period && 
                        !sp.IsDeleted)
            .OrderByDescending(sp => sp.PeriodEnd)
            .ThenBy(sp => sp.MetricType)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPerformance>> GetByDateRange(string supplierId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && 
                        sp.PeriodStart >= startDate && 
                        sp.PeriodEnd <= endDate && 
                        !sp.IsDeleted)
            .OrderByDescending(sp => sp.PeriodEnd)
            .ThenBy(sp => sp.MetricType)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageScore(string supplierId, PerformanceMetricType metricType)
    {
        var scores = await _dbSet
            .Where(sp => sp.SupplierId == supplierId && 
                        sp.MetricType == metricType && 
                        !sp.IsDeleted)
            .Select(sp => sp.Score)
            .ToListAsync();

        return scores.Any() ? scores.Average() : 0;
    }
}