using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoMemberRepository : Repository<SaccoMember>, ISaccoMemberRepository
{
    public SaccoMemberRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SaccoMember>> GetMembersBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Where(m => !m.IsDeleted && m.SaccoId == saccoId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByIdNumberAsync(string idNumber)
    {
        return await _dbSet.AnyAsync(m => !m.IsDeleted && m.IdNumber == idNumber);
    }

    public async Task<bool> ExistsByMemberNumberAsync(string memberNumber)
    {
        return await _dbSet.AnyAsync(m => !m.IsDeleted && m.MemberNumber == memberNumber);
    }

    public async Task<SaccoMember?> GetByMemberNumberAsync(string memberNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(m => !m.IsDeleted && m.MemberNumber == memberNumber);
    }

    public async Task<IEnumerable<SaccoMember>> SearchMembersAsync(string saccoId, string searchTerm)
    {
        return await _dbSet
            .Where(m => !m.IsDeleted && m.SaccoId == saccoId &&
                   (m.Name.Contains(searchTerm) ||
                    m.MemberNumber.Contains(searchTerm) ||
                    m.IdNumber.Contains(searchTerm) ||
                    m.ContactEmail.Contains(searchTerm)))
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<int> GetMemberCountBySaccoAsync(string saccoId)
    {
        return await _dbSet.CountAsync(m => !m.IsDeleted && m.SaccoId == saccoId);
    }

    public async Task<SaccoMember?> GetMemberWithDetailsAsync(string memberId)
    {
        return await _dbSet
            .Include(m => m.Shares.Where(s => !s.IsDeleted))
            .Include(m => m.Loans.Where(l => !l.IsDeleted))
            .Include(m => m.Memberships.Where(ms => !ms.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == memberId && !m.IsDeleted);
    }
}