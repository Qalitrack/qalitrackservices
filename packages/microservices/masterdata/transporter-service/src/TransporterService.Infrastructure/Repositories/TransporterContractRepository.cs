using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterContractRepository : Repository<TransporterContract>, ITransporterContractRepository
{
    public TransporterContractRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterContract>> GetContractsByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && !c.IsDeleted)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterContract>> GetContractsByTypeAsync(string transporterId, ContractType type)
    {
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && c.ContractType == type && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterContract>> GetActiveContractsAsync(string transporterId)
    {
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && c.Status == ContractStatus.Active && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterContract>> GetExpiringContractsAsync(string transporterId, int daysAhead = 30)
    {
        var cutoffDate = DateTime.Today.AddDays(daysAhead);
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && 
                       c.EndDate <= cutoffDate && 
                       c.Status == ContractStatus.Active && 
                       !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterContract>> GetExpiredContractsAsync(string transporterId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && 
                       c.EndDate < today && 
                       !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<TransporterContract?> GetByContractNumberAsync(string contractNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber && !c.IsDeleted);
    }

    public async Task<IEnumerable<TransporterContract>> GetContractsByClientAsync(string clientName)
    {
        return await _dbSet
            .Where(c => c.ClientName.Contains(clientName) && !c.IsDeleted)
            .ToListAsync();
    }
}