using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoRepository : Repository<Sacco>, ISaccoRepository
{
    public SaccoRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Sacco>> SearchSaccosAsync(string searchTerm)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && 
                   (s.Name.Contains(searchTerm) || 
                    s.RegistrationNumber.Contains(searchTerm) ||
                    s.ContactEmail.Contains(searchTerm)))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet.AnyAsync(s => !s.IsDeleted && s.RegistrationNumber == registrationNumber);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(s => !s.IsDeleted && s.Name == name);
    }

    public async Task<Sacco?> GetSaccoWithMembersAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Members.Where(m => !m.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Sacco?> GetSaccoWithFinancialAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Financial)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<Sacco>> GetSaccosByTypeAsync(SaccoType type)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && s.SaccoType == type)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sacco>> GetSaccosByStatusAsync(SaccoStatus status)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && s.Status == status)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}