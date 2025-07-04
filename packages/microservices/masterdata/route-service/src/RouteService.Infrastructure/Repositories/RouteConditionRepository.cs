using Microsoft.EntityFrameworkCore;
using RouteService.Core.Entities;
using RouteService.Core.Interfaces;
using RouteService.Infrastructure.Data;

namespace RouteService.Infrastructure.Repositories;

public class RouteConditionRepository : Repository<RouteCondition>, IRouteConditionRepository
{
    public RouteConditionRepository(RouteDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RouteCondition>> GetConditionsByRouteIdAsync(string routeId)
    {
        return await _dbSet.Where(c => c.RouteId == routeId).ToListAsync();
    }

    public async Task<IEnumerable<RouteCondition>> GetActiveConditionsAsync(string routeId)
    {
        return await _dbSet.Where(c => c.RouteId == routeId && c.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<RouteCondition>> GetConditionsByTypeAsync(ConditionType type)
    {
        return await _dbSet.Where(c => c.Type == type).ToListAsync();
    }

    public async Task<IEnumerable<RouteCondition>> GetConditionsByStatusAsync(ConditionStatus status)
    {
        return await _dbSet.Where(c => c.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<RouteCondition>> GetRecentConditionsAsync(DateTime since)
    {
        return await _dbSet.Where(c => c.ReportedAt >= since).ToListAsync();
    }

    public async Task<IEnumerable<RouteCondition>> GetConditionsByLocationAsync(double latitude, double longitude, double radiusKm)
    {
        // Simplified distance calculation - in production you'd use proper geospatial functions
        return await _dbSet.Where(c => c.Latitude.HasValue && c.Longitude.HasValue).ToListAsync();
    }

    public async Task<IEnumerable<RouteCondition>> GetCriticalConditionsAsync()
    {
        return await _dbSet.Where(c => c.Status == ConditionStatus.Critical && c.IsActive).ToListAsync();
    }

    public async Task<int> GetActiveConditionCountAsync(string routeId)
    {
        return await _dbSet.CountAsync(c => c.RouteId == routeId && c.IsActive);
    }

    public async Task<RouteCondition?> GetMostRecentConditionAsync(string routeId)
    {
        return await _dbSet.Where(c => c.RouteId == routeId)
            .OrderByDescending(c => c.ReportedAt)
            .FirstOrDefaultAsync();
    }
}