using RouteService.Core.Entities;

namespace RouteService.Core.Interfaces;

public interface IRouteScheduleRepository : IRepository<RouteSchedule>
{
    Task<IEnumerable<RouteSchedule>> GetSchedulesByRouteIdAsync(string routeId);
    Task<IEnumerable<RouteSchedule>> GetActiveSchedulesAsync(string routeId);
    Task<IEnumerable<RouteSchedule>> GetSchedulesByTypeAsync(ScheduleType type);
    Task<IEnumerable<RouteSchedule>> GetSchedulesByDateAsync(DateTime date);
    Task<IEnumerable<RouteSchedule>> GetRecurringSchedulesAsync();
    Task<IEnumerable<RouteSchedule>> GetSchedulesRequiringReservationAsync();
    Task<RouteSchedule?> GetNextScheduleAsync(string routeId, DateTime fromTime);
    Task<IEnumerable<RouteSchedule>> GetSchedulesByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime);
    Task<int> GetAvailableCapacityAsync(string scheduleId);
    Task<bool> IsReservationAvailableAsync(string scheduleId, DateTime requestedDate);
}