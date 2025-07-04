using Microsoft.EntityFrameworkCore;
using RouteService.Core.Entities;
using RouteService.Core.Interfaces;
using RouteService.Core.DTOs;
using RouteService.Infrastructure.Data;

namespace RouteService.Infrastructure.Repositories;

public class RouteRestrictionRepository : Repository<RouteRestriction>, IRouteRestrictionRepository
{
    public RouteRestrictionRepository(RouteDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RouteRestriction>> GetRestrictionsByRouteIdAsync(string routeId)
    {
        return await _dbSet
            .Where(r => r.RouteId == routeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteRestriction>> GetActiveRestrictionsAsync(string routeId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(r => r.RouteId == routeId && 
                       r.IsActive && 
                       (r.EffectiveDate == null || r.EffectiveDate <= now) &&
                       (r.ExpirationDate == null || r.ExpirationDate >= now))
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteRestriction>> GetRestrictionsByTypeAsync(RestrictionType type)
    {
        return await _dbSet
            .Where(r => r.Type == type && r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteRestriction>> GetRestrictionsBySeverityAsync(RestrictionSeverity severity)
    {
        return await _dbSet
            .Where(r => r.Severity == severity && r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteRestriction>> GetTimeBasedRestrictionsAsync(DateTime checkTime)
    {
        var timeOnly = TimeOnly.FromDateTime(checkTime);
        var dayOfWeek = checkTime.DayOfWeek.ToString();

        return await _dbSet
            .Where(r => r.IsActive &&
                       (r.StartTime == null || r.StartTime <= timeOnly) &&
                       (r.EndTime == null || r.EndTime >= timeOnly) &&
                       (r.DaysOfWeek == null || r.DaysOfWeek.Contains(dayOfWeek)))
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteRestriction>> GetVehicleRestrictionsAsync(VehicleSpecifications vehicle)
    {
        return await _dbSet
            .Where(r => r.IsActive &&
                       ((r.MaxWeight != null && vehicle.GrossWeight > r.MaxWeight) ||
                        (r.MaxHeight != null && vehicle.Height > r.MaxHeight) ||
                        (r.MaxWidth != null && vehicle.Width > r.MaxWidth) ||
                        (r.MaxLength != null && vehicle.Length > r.MaxLength) ||
                        (r.VehicleTypes != null && r.VehicleTypes.Contains(vehicle.VehicleType))))
            .ToListAsync();
    }

    public async Task<IEnumerable<RouteRestriction>> GetHazmatRestrictionsAsync(List<string> hazmatClasses)
    {
        return await _dbSet
            .Where(r => r.IsActive && 
                       r.HazmatClasses != null &&
                       hazmatClasses.Any(hc => r.HazmatClasses.Contains(hc)))
            .ToListAsync();
    }

    public async Task<bool> IsRouteRestrictedForVehicleAsync(string routeId, VehicleSpecifications vehicle, DateTime checkTime)
    {
        // Check vehicle restrictions
        var vehicleRestrictions = await _dbSet
            .Where(r => r.RouteId == routeId && r.IsActive &&
                       ((r.MaxWeight != null && vehicle.GrossWeight > r.MaxWeight) ||
                        (r.MaxHeight != null && vehicle.Height > r.MaxHeight) ||
                        (r.MaxWidth != null && vehicle.Width > r.MaxWidth) ||
                        (r.MaxLength != null && vehicle.Length > r.MaxLength) ||
                        (r.VehicleTypes != null && r.VehicleTypes.Contains(vehicle.VehicleType))))
            .AnyAsync();

        if (vehicleRestrictions)
            return true;

        // Check time-based restrictions
        var timeOnly = TimeOnly.FromDateTime(checkTime);
        var dayOfWeek = checkTime.DayOfWeek.ToString();

        var timeRestrictions = await _dbSet
            .Where(r => r.RouteId == routeId && r.IsActive &&
                       r.StartTime != null && r.EndTime != null &&
                       r.StartTime <= timeOnly && r.EndTime >= timeOnly &&
                       (r.DaysOfWeek == null || r.DaysOfWeek.Contains(dayOfWeek)))
            .AnyAsync();

        if (timeRestrictions)
            return true;

        // Check hazmat restrictions
        if (vehicle.HazmatClasses.Any())
        {
            var hazmatRestrictions = await _dbSet
                .Where(r => r.RouteId == routeId && r.IsActive &&
                           r.HazmatClasses != null &&
                           vehicle.HazmatClasses.Any(hc => r.HazmatClasses.Contains(hc)))
                .AnyAsync();

            if (hazmatRestrictions)
                return true;
        }

        return false;
    }
}