using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeConfigurationRepository : IRepository<WeighbridgeConfiguration>
{
    Task<WeighbridgeConfiguration?> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeConfiguration>> GetByConfigurationNameAsync(string configurationName);
}