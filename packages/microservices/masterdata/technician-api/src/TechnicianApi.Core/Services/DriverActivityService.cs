using AutoMapper;
using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class DriverActivityService : IDriverActivityService
{
    private readonly IRepository<DriverActivity> _repository;
    private readonly IMapper _mapper;

    public DriverActivityService(IRepository<DriverActivity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task LogActivityAsync(
        string driverId,
        DriverActivityType activityType,
        string? activityData = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var activity = new DriverActivity
        {
            DriverId = driverId,
            ActivityType = activityType,
            ActivityData = activityData,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _repository.CreateAsync(activity);
    }

    public async Task<IEnumerable<DriverActivityResponseDto>> GetDriverActivitiesAsync(string driverId, int limit = 100)
    {
        var activities = await _repository.FindAsync(a => a.DriverId == driverId);
        var sorted = activities.OrderByDescending(a => a.CreatedAt).Take(limit);
        return _mapper.Map<IEnumerable<DriverActivityResponseDto>>(sorted);
    }
}
