using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IWeightCorrectionRepository : IRepository<WeightCorrection>
{
    Task<IEnumerable<WeightCorrection>> GetByMeasurementIdAsync(Guid measurementId);
    Task<IEnumerable<WeightCorrection>> GetPendingApprovalsAsync(string organizationId);
    Task<IEnumerable<WeightCorrection>> GetByAuthorizedByAsync(string authorizedBy);
    Task<IEnumerable<WeightCorrection>> GetRecentCorrectionsAsync(string organizationId, int days = 30);
}