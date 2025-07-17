namespace UserService.Core.Interfaces;

public interface IUserStatusRepository
{
    Task UpdateUserStatusAsync(string userId, bool isActive);
}