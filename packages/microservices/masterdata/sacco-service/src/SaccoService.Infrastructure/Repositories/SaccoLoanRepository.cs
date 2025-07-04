using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoLoanRepository : Repository<SaccoLoan>, ISaccoLoanRepository
{
    public SaccoLoanRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SaccoLoan>> GetLoansBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Include(l => l.Member)
            .Include(l => l.ApprovedBy)
            .Where(l => !l.IsDeleted && l.SaccoId == saccoId)
            .OrderByDescending(l => l.ApplicationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaccoLoan>> GetLoansByMemberAsync(string memberId)
    {
        return await _dbSet
            .Include(l => l.Sacco)
            .Include(l => l.ApprovedBy)
            .Where(l => !l.IsDeleted && l.MemberId == memberId)
            .OrderByDescending(l => l.ApplicationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaccoLoan>> GetLoansByStatusAsync(string saccoId, LoanStatus status)
    {
        return await _dbSet
            .Include(l => l.Member)
            .Include(l => l.ApprovedBy)
            .Where(l => !l.IsDeleted && l.SaccoId == saccoId && l.Status == status)
            .OrderByDescending(l => l.ApplicationDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalOutstandingBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && 
                   l.SaccoId == saccoId && 
                   (l.Status == LoanStatus.Active || l.Status == LoanStatus.Disbursed))
            .SumAsync(l => l.OutstandingBalance);
    }

    public async Task<decimal> GetTotalOutstandingByMemberAsync(string memberId)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && 
                   l.MemberId == memberId && 
                   (l.Status == LoanStatus.Active || l.Status == LoanStatus.Disbursed))
            .SumAsync(l => l.OutstandingBalance);
    }

    public async Task<bool> ExistsByLoanNumberAsync(string loanNumber)
    {
        return await _dbSet.AnyAsync(l => !l.IsDeleted && l.LoanNumber == loanNumber);
    }

    public async Task<SaccoLoan?> GetByLoanNumberAsync(string loanNumber)
    {
        return await _dbSet
            .Include(l => l.Member)
            .Include(l => l.Sacco)
            .Include(l => l.ApprovedBy)
            .FirstOrDefaultAsync(l => !l.IsDeleted && l.LoanNumber == loanNumber);
    }
}