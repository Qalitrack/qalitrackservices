using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : Repository<UserService.Core.Entities.User>, IUserRepository
{
    public UserRepository(UserServiceDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<UserService.Core.Entities.User?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}