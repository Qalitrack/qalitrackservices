using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IRealTimeStreamingService
{
    Task<RealTimeSessionDto> StartStreamingSessionAsync(CreateRealTimeSessionDto createDto, string organizationId, string userId);
    Task<RealTimeSessionDto?> GetStreamingSessionAsync(string sessionId, string organizationId);
    Task<bool> UpdateStreamingSessionAsync(string sessionId, UpdateRealTimeSessionDto updateDto, string organizationId);
    Task<bool> EndStreamingSessionAsync(string sessionId, string organizationId, string userId);
    Task<List<RealTimeSessionDto>> GetActiveSessionsAsync(string organizationId);
    Task<List<StreamingWeightDataDto>> GetStreamingDataAsync(string sessionId, DateTime? fromTime = null);
    Task PublishWeightDataAsync(StreamingWeightDataDto weightData);
    Task<bool> PauseStreamingAsync(string sessionId, string organizationId);
    Task<bool> ResumeStreamingAsync(string sessionId, string organizationId);
}