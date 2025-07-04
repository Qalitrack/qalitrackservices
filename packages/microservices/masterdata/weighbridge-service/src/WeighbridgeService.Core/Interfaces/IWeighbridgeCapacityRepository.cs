using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeCapacityRepository : IRepository<WeighbridgeCapacity>
{
    Task<WeighbridgeCapacity?> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeCapacity>> GetAvailableCapacitiesAsync();
    Task<IEnumerable<WeighbridgeCapacity>> GetOverCapacityAsync();
}