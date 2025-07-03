using Microsoft.EntityFrameworkCore;
using AnalyticsService.Core.Entities;
using AnalyticsService.Core.Interfaces;
using AnalyticsService.Infrastructure.Data;

namespace AnalyticsService.Infrastructure.Repositories;

public class AnalyticsMetricRepository : AnalyticsRepository<AnalyticsMetric>, IAnalyticsMetricRepository
{
    public AnalyticsMetricRepository(AnalyticsDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetByOrganizationAsync(string organizationId, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(m => m.OrganizationId == organizationId);

        if (from.HasValue)
            query = query.Where(m => m.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Timestamp <= to.Value);

        return await query.OrderByDescending(m => m.Timestamp).ToListAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetByMetricTypeAsync(string organizationId, string metricType, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(m => m.OrganizationId == organizationId && m.MetricType == metricType);

        if (from.HasValue)
            query = query.Where(m => m.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Timestamp <= to.Value);

        return await query.OrderByDescending(m => m.Timestamp).ToListAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetByWeighbridgeAsync(string weighbridgeId, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(m => m.WeighbridgeId == weighbridgeId);

        if (from.HasValue)
            query = query.Where(m => m.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Timestamp <= to.Value);

        return await query.OrderByDescending(m => m.Timestamp).ToListAsync();
    }

    public async Task<AnalyticsMetric?> GetLatestMetricAsync(string organizationId, string metricType, string? weighbridgeId = null)
    {
        var query = _dbSet.Where(m => m.OrganizationId == organizationId && m.MetricType == metricType);

        if (!string.IsNullOrEmpty(weighbridgeId))
            query = query.Where(m => m.WeighbridgeId == weighbridgeId);

        return await query.OrderByDescending(m => m.Timestamp).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetTimeSeriesAsync(string organizationId, string metricType, DateTime from, DateTime to, string? weighbridgeId = null)
    {
        var query = _dbSet.Where(m => 
            m.OrganizationId == organizationId && 
            m.MetricType == metricType && 
            m.Timestamp >= from && 
            m.Timestamp <= to);

        if (!string.IsNullOrEmpty(weighbridgeId))
            query = query.Where(m => m.WeighbridgeId == weighbridgeId);

        return await query.OrderBy(m => m.Timestamp).ToListAsync();
    }
}