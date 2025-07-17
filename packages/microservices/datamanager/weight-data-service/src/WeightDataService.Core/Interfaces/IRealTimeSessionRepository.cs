using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IRealTimeSessionRepository : IRepository<RealTimeSession>
{
    Task<List<RealTimeSession>> GetActiveSessionsAsync(string organizationId);
    Task<RealTimeSession?> GetSessionByIdAsync(string sessionId, string organizationId);
    Task<List<RealTimeSession>> GetSessionsByWeighbridgeAsync(string weighbridgeId, string organizationId);
    Task<bool> UpdateSessionStatusAsync(string sessionId, StreamingStatus status);
}