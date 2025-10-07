using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Infrastructure.Data;

namespace TechnicianApi.Infrastructure.Repositories;

public class TechnicianRepository : Repository<TechnicianApi.Core.Entities.Technician>, ITechnicianRepository
{
    public TechnicianRepository(TechnicianApiDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<TechnicianApi.Core.Entities.Technician?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}