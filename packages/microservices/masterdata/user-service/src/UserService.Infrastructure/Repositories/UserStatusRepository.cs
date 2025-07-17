using Microsoft.EntityFrameworkCore;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

public class UserStatusRepository : IUserStatusRepository
{
    private readonly UserServiceDbContext _context;

    public UserStatusRepository(UserServiceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task UpdateUserStatusAsync(string userId, bool isActive)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // You could add more methods if needed to handle more status-related logic, e.g. status history tracking
}