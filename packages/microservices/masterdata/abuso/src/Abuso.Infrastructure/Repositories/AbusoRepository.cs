using Microsoft.EntityFrameworkCore;
using Abuso.Core.Entities;
using Abuso.Core.Interfaces;
using Abuso.Infrastructure.Data;

namespace Abuso.Infrastructure.Repositories;

public class AbusoRepository : Repository<Abuso.Core.Entities.Abuso>, IAbusoRepository
{
    public AbusoRepository(AbusoDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<Abuso.Core.Entities.Abuso?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}