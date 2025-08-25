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
        var periodString = period.ToString();
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && 
                        sp.Period == periodString && 
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

    public async Task<IEnumerable<SupplierPerformance>> GetBySupplierIdAsync(string supplierId)
    {
        return await GetBySupplierId(supplierId);
    }

    public async Task<SupplierPerformance?> GetBySupplierAndPeriodAsync(string supplierId, int year, int month)
    {
        return await _dbSet
            .FirstOrDefaultAsync(sp => sp.SupplierId == supplierId && 
                                     sp.Year == year && 
                                     sp.Month == month && 
                                     !sp.IsDeleted);
    }

    public async Task<IEnumerable<SupplierPerformance>> GetByPeriodAsync(int year, int? month = null)
    {
        var query = _dbSet.Where(sp => sp.Year == year && !sp.IsDeleted);
        
        if (month.HasValue)
        {
            query = query.Where(sp => sp.Month == month.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null)
    {
        var query = _dbSet.Where(sp => sp.SupplierId == supplierId && !sp.IsDeleted);
        
        if (months.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddMonths(-months.Value);
            query = query.Where(sp => sp.CreatedAt >= cutoffDate);
        }

        var ratings = await query
            .Where(sp => sp.OverallRating.HasValue)
            .Select(sp => sp.OverallRating.Value)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : null;
    }

    public async Task<SupplierPerformance?> GetLatestPerformanceAsync(string supplierId)
    {
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && !sp.IsDeleted)
            .OrderByDescending(sp => sp.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Supplier>> GetTopPerformersAsync(int count = 10)
    {
        var topPerformerIds = await _dbSet
            .Where(sp => !sp.IsDeleted && sp.OverallRating.HasValue)
            .GroupBy(sp => sp.SupplierId)
            .Select(g => new { SupplierId = g.Key, AvgRating = g.Average(sp => sp.OverallRating.Value) })
            .OrderByDescending(x => x.AvgRating)
            .Take(count)
            .Select(x => x.SupplierId)
            .ToListAsync();

        return await _context.Suppliers
            .Where(s => topPerformerIds.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> GetPoorPerformersAsync(int count = 10)
    {
        var poorPerformerIds = await _dbSet
            .Where(sp => !sp.IsDeleted && sp.OverallRating.HasValue)
            .GroupBy(sp => sp.SupplierId)
            .Select(g => new { SupplierId = g.Key, AvgRating = g.Average(sp => sp.OverallRating.Value) })
            .OrderBy(x => x.AvgRating)
            .Take(count)
            .Select(x => x.SupplierId)
            .ToListAsync();

        return await _context.Suppliers
            .Where(s => poorPerformerIds.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync();
    }
}