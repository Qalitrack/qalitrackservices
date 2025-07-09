using Microsoft.EntityFrameworkCore;
using TestServiceV4.Core.Entities;
using TestServiceV4.Core.Interfaces;
using TestServiceV4.Infrastructure.Data;

namespace TestServiceV4.Infrastructure.Repositories;

public class TestentityRepository : Repository<Testentity>, ITestentityRepository
{
    public TestentityRepository(TestServiceV4DbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<Testentity?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}