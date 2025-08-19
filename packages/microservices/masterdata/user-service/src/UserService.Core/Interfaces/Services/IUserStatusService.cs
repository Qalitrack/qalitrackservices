namespace UserService.Core.Interfaces.Services;

public interface IUserStatusService
{
    void EnqueueStatusUpdate(string userId, bool isActive);
}
