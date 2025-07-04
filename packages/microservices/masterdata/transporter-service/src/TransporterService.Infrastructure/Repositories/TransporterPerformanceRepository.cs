using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterPerformanceRepository : Repository<TransporterPerformance>, ITransporterPerformanceRepository
{
    public TransporterPerformanceRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterPerformance>> GetPerformanceByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(p => p.TransporterId == transporterId && !p.IsDeleted)
            .OrderByDescending(p => p.RecordDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterPerformance>> GetPerformanceByTypeAsync(string transporterId, PerformanceMetricType metricType)
    {
        return await _dbSet
            .Where(p => p.TransporterId == transporterId && p.MetricType == metricType && !p.IsDeleted)
            .OrderByDescending(p => p.RecordDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterPerformance>> GetPerformanceByPeriodAsync(string transporterId, string period)
    {
        return await _dbSet
            .Where(p => p.TransporterId == transporterId && p.Period == period && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterPerformance>> GetPerformanceByDateRangeAsync(string transporterId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(p => p.TransporterId == transporterId && 
                       p.RecordDate >= startDate && 
                       p.RecordDate <= endDate && 
                       !p.IsDeleted)
            .OrderByDescending(p => p.RecordDate)
            .ToListAsync();
    }

    public async Task<TransporterPerformance?> GetLatestPerformanceAsync(string transporterId, PerformanceMetricType metricType)
    {
        return await _dbSet
            .Where(p => p.TransporterId == transporterId && p.MetricType == metricType && !p.IsDeleted)
            .OrderByDescending(p => p.RecordDate)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetAveragePerformanceAsync(string transporterId, PerformanceMetricType metricType, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(p => p.TransporterId == transporterId && p.MetricType == metricType && !p.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(p => p.RecordDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.RecordDate <= endDate.Value);

        var performances = await query.ToListAsync();
        return performances.Any() ? performances.Average(p => p.Value) : 0;
    }
}