using Microsoft.EntityFrameworkCore;
using {{ServiceName}}.Core.Entities;
using {{ServiceName}}.Core.Interfaces;
using {{ServiceName}}.Infrastructure.Data;

namespace {{ServiceName}}.Infrastructure.Repositories;

public class {{EntityName}}Repository : Repository<{{ServiceName}}.Core.Entities.{{EntityName}}>, I{{EntityName}}Repository
{
    public {{EntityName}}Repository({{ServiceName}}DbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<{{ServiceName}}.Core.Entities.{{EntityName}}?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}