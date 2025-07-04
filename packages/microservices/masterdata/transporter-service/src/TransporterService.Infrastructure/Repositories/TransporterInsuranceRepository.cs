using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterInsuranceRepository : Repository<TransporterInsurance>, ITransporterInsuranceRepository
{
    public TransporterInsuranceRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterInsurance>> GetInsuranceByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(i => i.TransporterId == transporterId && !i.IsDeleted)
            .OrderBy(i => i.InsuranceType)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterInsurance>> GetInsuranceByTypeAsync(string transporterId, InsuranceType type)
    {
        return await _dbSet
            .Where(i => i.TransporterId == transporterId && i.InsuranceType == type && !i.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterInsurance>> GetActiveInsuranceAsync(string transporterId)
    {
        return await _dbSet
            .Where(i => i.TransporterId == transporterId && i.Status == InsuranceStatus.Active && !i.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterInsurance>> GetExpiringInsuranceAsync(string transporterId, int daysAhead = 30)
    {
        var cutoffDate = DateTime.Today.AddDays(daysAhead);
        return await _dbSet
            .Where(i => i.TransporterId == transporterId && 
                       i.PolicyEndDate <= cutoffDate && 
                       i.Status == InsuranceStatus.Active && 
                       !i.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterInsurance>> GetExpiredInsuranceAsync(string transporterId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(i => i.TransporterId == transporterId && 
                       i.PolicyEndDate < today && 
                       !i.IsDeleted)
            .ToListAsync();
    }

    public async Task<TransporterInsurance?> GetByPolicyNumberAsync(string policyNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.PolicyNumber == policyNumber && !i.IsDeleted);
    }
}