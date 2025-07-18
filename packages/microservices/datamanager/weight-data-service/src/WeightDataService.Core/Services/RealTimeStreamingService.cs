using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class RealTimeStreamingService : IRealTimeStreamingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RealTimeStreamingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RealTimeSessionDto> StartStreamingSessionAsync(CreateRealTimeSessionDto createDto, string organizationId, string userId)
    {
        var session = new RealTimeSession
        {
            SessionId = Guid.NewGuid().ToString(),
            WeighbridgeId = createDto.WeighbridgeId,
            VehicleRegistration = createDto.VehicleRegistration,
            SampleRate = createDto.SampleRate,
            StabilityThreshold = createDto.StabilityThreshold,
            OrganizationId = organizationId,
            CreatedBy = userId
        };

        await _unitOfWork.RealTimeSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RealTimeSessionDto>(session);
    }

    public async Task<RealTimeSessionDto?> GetStreamingSessionAsync(string sessionId, string organizationId)
    {
        var session = await _unitOfWork.RealTimeSessions.GetSessionByIdAsync(sessionId, organizationId);
        return session != null ? _mapper.Map<RealTimeSessionDto>(session) : null;
    }

    public async Task<bool> UpdateStreamingSessionAsync(string sessionId, UpdateRealTimeSessionDto updateDto, string organizationId)
    {
        var session = await _unitOfWork.RealTimeSessions.GetSessionByIdAsync(sessionId, organizationId);
        if (session == null) return false;

        session.CurrentWeight = updateDto.CurrentWeight;
        session.IsStable = updateDto.IsStable;
        session.SampleCount++;
        session.UpdatedAt = DateTime.UtcNow;

        if (updateDto.CurrentWeight < session.MinWeight || session.MinWeight == 0)
            session.MinWeight = updateDto.CurrentWeight;
        
        if (updateDto.CurrentWeight > session.MaxWeight)
            session.MaxWeight = updateDto.CurrentWeight;

        session.AverageWeight = (session.AverageWeight * (session.SampleCount - 1) + updateDto.CurrentWeight) / session.SampleCount;

        if (!string.IsNullOrEmpty(updateDto.EventData))
        {
            session.EventData = updateDto.EventData;
        }

        await _unitOfWork.SaveChangesAsync();

        // Publish real-time data
        var streamingData = new StreamingWeightDataDto
        {
            SessionId = sessionId,
            Weight = updateDto.CurrentWeight,
            Timestamp = DateTime.UtcNow,
            IsStable = updateDto.IsStable,
            EventType = "WeightUpdate"
        };
        await PublishWeightDataAsync(streamingData);

        return true;
    }

    public async Task<bool> EndStreamingSessionAsync(string sessionId, string organizationId, string userId)
    {
        var session = await _unitOfWork.RealTimeSessions.GetSessionByIdAsync(sessionId, organizationId);
        if (session == null) return false;

        session.EndTime = DateTime.UtcNow;
        session.Status = StreamingStatus.Completed;
        session.UpdatedBy = userId;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<RealTimeSessionDto>> GetActiveSessionsAsync(string organizationId)
    {
        var sessions = await _unitOfWork.RealTimeSessions.GetActiveSessionsAsync(organizationId);
        return _mapper.Map<List<RealTimeSessionDto>>(sessions);
    }

    public async Task<List<StreamingWeightDataDto>> GetStreamingDataAsync(string sessionId, DateTime? fromTime = null)
    {
        var session = await _unitOfWork.RealTimeSessions.GetSessionByIdAsync(sessionId, string.Empty);
        if (session == null) return new List<StreamingWeightDataDto>();

        var measurements = session.Measurements
            .Where(m => fromTime == null || m.MeasurementDateTime >= fromTime)
            .OrderBy(m => m.MeasurementDateTime)
            .Select(m => new StreamingWeightDataDto
            {
                SessionId = sessionId,
                Weight = m.Weight,
                Timestamp = m.MeasurementDateTime,
                IsStable = session.IsStable,
                EventType = "Measurement"
            })
            .ToList();

        return measurements;
    }

    public async Task PublishWeightDataAsync(StreamingWeightDataDto weightData)
    {
        // Implementation for SignalR or message queue publishing
        // For now, this is a placeholder that could integrate with:
        // 1. SignalR Hub for real-time browser notifications
        // 2. Message queue (RabbitMQ, Azure Service Bus) for microservice communication
        // 3. WebSocket connections for direct client communication
        
        // Example SignalR implementation:
        // await _hubContext.Clients.Group($"weighbridge_{weightData.SessionId}")
        //     .SendAsync("WeightDataReceived", weightData);
        
        // Example message queue implementation:
        // await _messagePublisher.PublishAsync("weight-data-updates", weightData);
        
        await Task.CompletedTask; // Placeholder
    }

    public async Task<bool> PauseStreamingAsync(string sessionId, string organizationId)
    {
        return await _unitOfWork.RealTimeSessions.UpdateSessionStatusAsync(sessionId, StreamingStatus.Paused);
    }

    public async Task<bool> ResumeStreamingAsync(string sessionId, string organizationId)
    {
        return await _unitOfWork.RealTimeSessions.UpdateSessionStatusAsync(sessionId, StreamingStatus.Active);
    }
}