using Microsoft.EntityFrameworkCore;
using AnalyticsService.Core.Entities;
using AnalyticsService.Core.Interfaces;
using AnalyticsService.Infrastructure.Data;

namespace AnalyticsService.Infrastructure.Repositories;

public class AnomalyRepository : AnalyticsRepository<Anomaly>, IAnomalyRepository
{
    public AnomalyRepository(AnalyticsDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Anomaly>> GetByOrganizationAsync(string organizationId, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(a => a.OrganizationId == organizationId);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }

    public async Task<IEnumerable<Anomaly>> GetByMetricTypeAsync(string organizationId, string metricType, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(a => a.OrganizationId == organizationId && a.MetricType == metricType);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }

    public async Task<IEnumerable<Anomaly>> GetBySeverityAsync(string organizationId, string severity)
    {
        return await _dbSet
            .Where(a => a.OrganizationId == organizationId && a.Severity == severity)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Anomaly>> GetOpenAnomaliesAsync(string organizationId)
    {
        return await _dbSet
            .Where(a => a.OrganizationId == organizationId && a.Status == "Open")
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Anomaly>> GetUnnotifiedAnomaliesAsync()
    {
        return await _dbSet
            .Where(a => !a.IsNotified)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<int> GetAnomalyCountAsync(string organizationId, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(a => a.OrganizationId == organizationId);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.CountAsync();
    }
}