using Microsoft.EntityFrameworkCore;
using RouteService.Core.Entities;
using RouteService.Core.Interfaces;
using RouteService.Infrastructure.Data;

namespace RouteService.Infrastructure.Repositories;

public class RouteWaypointRepository : Repository<RouteWaypoint>, IRouteWaypointRepository
{
    public RouteWaypointRepository(RouteDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RouteWaypoint>> GetWaypointsByRouteIdAsync(string routeId)
    {
        return await _dbSet
            .Where(w => w.RouteId == routeId)
            .OrderBy(w => w.Sequence)
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteWaypoint>> GetWaypointsByTypeAsync(WaypointType type)
    {
        return await _dbSet
            .Where(w => w.Type == type)
            .ToListAsync();
    }

    public async Task<RouteWaypoint?> GetNextWaypointAsync(string routeId, int currentSequence)
    {
        return await _dbSet
            .Where(w => w.RouteId == routeId && w.Sequence > currentSequence)
            .OrderBy(w => w.Sequence)
            .FirstOrDefaultAsync();
    }

    public async Task<RouteWaypoint?> GetPreviousWaypointAsync(string routeId, int currentSequence)
    {
        return await _dbSet
            .Where(w => w.RouteId == routeId && w.Sequence < currentSequence)
            .OrderByDescending(w => w.Sequence)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RouteWaypoint>> GetRequiredWaypointsAsync(string routeId)
    {
        return await _dbSet
            .Where(w => w.RouteId == routeId && w.IsRequired)
            .OrderBy(w => w.Sequence)
            .ToListAsync();
    }

    public async Task<int> GetMaxSequenceForRouteAsync(string routeId)
    {
        var maxSequence = await _dbSet
            .Where(w => w.RouteId == routeId)
            .MaxAsync(w => (int?)w.Sequence);
        
        return maxSequence ?? 0;
    }

    public async Task ResequenceWaypointsAsync(string routeId)
    {
        var waypoints = await _dbSet
            .Where(w => w.RouteId == routeId && !w.IsDeleted)
            .OrderBy(w => w.Sequence)
            .ToListAsync();

        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].Sequence = i + 1;
            waypoints[i].UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}