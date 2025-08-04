using Microsoft.EntityFrameworkCore;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserStatusRepository(UserServiceDbContext context) : IUserStatusRepository
{
    private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task UpdateUserStatusAsync(string userId, bool isActive)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;
            await _context.SaveChangesAsync();
        }
    }

}