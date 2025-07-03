using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeRepository : IRepository<Weighbridge>
{
    Task<IEnumerable<Weighbridge>> GetActiveWeighbridgesAsync();
    Task<IEnumerable<Weighbridge>> GetWeighbridgesByStatusAsync(WeighbridgeStatus status);
    Task<Weighbridge?> GetByCodeAsync(string code);
    Task<IEnumerable<Weighbridge>> GetWeighbridgesByLocationAsync(string location);
    Task<IEnumerable<Weighbridge>> GetWeighbridgesNeedingCalibrationAsync();
    Task<IEnumerable<Weighbridge>> GetWeighbridgesNeedingMaintenanceAsync();
    Task<IEnumerable<Weighbridge>> GetAvailableWeighbridgesAsync(DateTime requestedTime);
}