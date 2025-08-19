namespace UserService.Core.Interfaces.Repositories;

public interface IUserStatusRepository
{
    Task UpdateUserStatusAsync(string userId, bool isActive);
}