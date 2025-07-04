using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeLocationRepository : IRepository<WeighbridgeLocation>
{
    Task<WeighbridgeLocation?> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeLocation>> GetByCityAsync(string city);
    Task<IEnumerable<WeighbridgeLocation>> GetByStateAsync(string state);
    Task<IEnumerable<WeighbridgeLocation>> GetByCountryAsync(string country);
    Task<IEnumerable<WeighbridgeLocation>> GetActiveLocationsAsync();
}