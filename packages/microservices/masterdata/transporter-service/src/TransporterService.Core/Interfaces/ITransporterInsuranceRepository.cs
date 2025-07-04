using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterInsuranceRepository : IRepository<TransporterInsurance>
{
    Task<IEnumerable<TransporterInsurance>> GetInsuranceByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterInsurance>> GetInsuranceByTypeAsync(string transporterId, InsuranceType type);
    Task<IEnumerable<TransporterInsurance>> GetActiveInsuranceAsync(string transporterId);
    Task<IEnumerable<TransporterInsurance>> GetExpiringInsuranceAsync(string transporterId, int daysAhead = 30);
    Task<IEnumerable<TransporterInsurance>> GetExpiredInsuranceAsync(string transporterId);
    Task<TransporterInsurance?> GetByPolicyNumberAsync(string policyNumber);
}