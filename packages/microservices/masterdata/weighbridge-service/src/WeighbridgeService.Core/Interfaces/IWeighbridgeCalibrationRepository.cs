using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeCalibrationRepository : IRepository<WeighbridgeCalibration>
{
    Task<IEnumerable<WeighbridgeCalibration>> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<WeighbridgeCalibration?> GetLatestCalibrationAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeCalibration>> GetByStatusAsync(CalibrationStatus status);
    Task<IEnumerable<WeighbridgeCalibration>> GetByTypeAsync(CalibrationType type);
    Task<IEnumerable<WeighbridgeCalibration>> GetScheduledCalibrationsAsync();
    Task<IEnumerable<WeighbridgeCalibration>> GetOverdueCalibrationsAsync();
}