using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoCommitteeRepository : Repository<SaccoCommittee>, ISaccoCommitteeRepository
{
    public SaccoCommitteeRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SaccoCommittee>> GetCommitteeBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Include(c => c.Member)
            .Where(c => !c.IsDeleted && c.SaccoId == saccoId)
            .OrderBy(c => c.Position)
            .ToListAsync();
    }

    public async Task<bool> ExistsByPositionAsync(string saccoId, CommitteePosition position)
    {
        return await _dbSet.AnyAsync(c => !c.IsDeleted && 
                                     c.SaccoId == saccoId && 
                                     c.Position == position && 
                                     c.Status == CommitteeStatus.Active);
    }

    public async Task<SaccoCommittee?> GetByPositionAsync(string saccoId, CommitteePosition position)
    {
        return await _dbSet
            .Include(c => c.Member)
            .FirstOrDefaultAsync(c => !c.IsDeleted && 
                               c.SaccoId == saccoId && 
                               c.Position == position && 
                               c.Status == CommitteeStatus.Active);
    }

    public async Task<IEnumerable<SaccoCommittee>> GetActiveCommitteeMembersAsync(string saccoId)
    {
        return await _dbSet
            .Include(c => c.Member)
            .Where(c => !c.IsDeleted && c.SaccoId == saccoId && c.Status == CommitteeStatus.Active)
            .OrderBy(c => c.Position)
            .ToListAsync();
    }
}