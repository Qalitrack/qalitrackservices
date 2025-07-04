using Microsoft.EntityFrameworkCore;
using RouteService.Core.Entities;
using RouteService.Core.Interfaces;
using RouteService.Core.DTOs;
using RouteService.Infrastructure.Data;

namespace RouteService.Infrastructure.Repositories;

public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(RouteDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Route>> GetRoutesBetweenAsync(string origin, string destination)
    {
        return await _dbSet
            .Where(r => r.Origin.Contains(origin) && r.Destination.Contains(destination))
            .Include(r => r.Waypoints)
            .Include(r => r.Restrictions)
            .Include(r => r.Conditions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
    {
        return await _dbSet
            .Where(r => r.Status == RouteStatus.Active)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesByTypeAsync(RouteType routeType)
    {
        return await _dbSet
            .Where(r => r.RouteType == routeType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesForVehicleAsync(VehicleSpecifications vehicle)
    {
        return await _dbSet
            .Where(r => (r.MaxVehicleWeight == null || r.MaxVehicleWeight >= vehicle.GrossWeight) &&
                       (r.MaxVehicleHeight == null || r.MaxVehicleHeight >= vehicle.Height) &&
                       (r.MaxVehicleWidth == null || r.MaxVehicleWidth >= vehicle.Width) &&
                       (r.MaxVehicleLength == null || r.MaxVehicleLength >= vehicle.Length))
            .ToListAsync();
    }

    public async Task<Route?> GetRouteByCodeAsync(string code)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Code == code);
    }

    public async Task<IEnumerable<Route>> GetRoutesWithWaypointsAsync()
    {
        return await _dbSet
            .Include(r => r.Waypoints.OrderBy(w => w.Sequence))
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesWithConditionsAsync()
    {
        return await _dbSet
            .Include(r => r.Conditions.Where(c => c.IsActive))
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesWithRestrictionsAsync()
    {
        return await _dbSet
            .Include(r => r.Restrictions.Where(res => res.IsActive))
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> SearchRoutesAsync(string searchTerm)
    {
        return await _dbSet
            .Where(r => r.Name.Contains(searchTerm) || 
                       r.Code.Contains(searchTerm) ||
                       r.Description!.Contains(searchTerm) ||
                       r.Origin.Contains(searchTerm) ||
                       r.Destination.Contains(searchTerm))
            .ToListAsync();
    }
}