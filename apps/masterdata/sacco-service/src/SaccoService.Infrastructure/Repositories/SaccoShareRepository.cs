using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoShareRepository : Repository<SaccoShare>, ISaccoShareRepository
{
    public SaccoShareRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SaccoShare>> GetSharesBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Include(s => s.Member)
            .Where(s => !s.IsDeleted && s.SaccoId == saccoId)
            .OrderByDescending(s => s.PurchaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaccoShare>> GetSharesByMemberAsync(string memberId)
    {
        return await _dbSet
            .Include(s => s.Sacco)
            .Where(s => !s.IsDeleted && s.MemberId == memberId)
            .OrderByDescending(s => s.PurchaseDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSharesBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && s.SaccoId == saccoId && s.Status == ShareStatus.Active)
            .SumAsync(s => s.TotalValue);
    }

    public async Task<decimal> GetTotalSharesByMemberAsync(string memberId)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && s.MemberId == memberId && s.Status == ShareStatus.Active)
            .SumAsync(s => s.TotalValue);
    }

    public async Task<bool> ExistsByCertificateNumberAsync(string certificateNumber)
    {
        return await _dbSet.AnyAsync(s => !s.IsDeleted && s.ShareCertificateNumber == certificateNumber);
    }
}