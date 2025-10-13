using Microsoft.EntityFrameworkCore;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Infrastructure.Data;

namespace Masterdata.Infrastructure.Repositories;

public class BaseRepository : Repository<Masterdata.Core.Entities.Base>, IBaseRepository
{
    public BaseRepository(MasterdataDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<Masterdata.Core.Entities.Base?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}