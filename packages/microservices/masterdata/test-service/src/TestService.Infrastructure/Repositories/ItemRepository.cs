using Microsoft.EntityFrameworkCore;
using TestService.Core.Entities;
using TestService.Core.Interfaces;
using TestService.Infrastructure.Data;

namespace TestService.Infrastructure.Repositories;

public class ItemRepository : Repository<Item>, IItemRepository
{
    public ItemRepository(TestServiceDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<Item?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}