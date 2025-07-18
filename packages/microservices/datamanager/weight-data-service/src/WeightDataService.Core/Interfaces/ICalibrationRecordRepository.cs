using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface ICalibrationRecordRepository : IRepository<CalibrationRecord>
{
    Task<List<CalibrationRecord>> GetByWeighbridgeIdAsync(string weighbridgeId, string organizationId);
    Task<CalibrationRecord?> GetLatestCalibrationAsync(string weighbridgeId, string organizationId);
    Task<List<CalibrationRecord>> GetCalibrationsDueAsync(string organizationId, DateTime? beforeDate = null);
    Task<List<CalibrationRecord>> GetCalibrationHistoryAsync(string weighbridgeId, string organizationId, int limit = 50);
}