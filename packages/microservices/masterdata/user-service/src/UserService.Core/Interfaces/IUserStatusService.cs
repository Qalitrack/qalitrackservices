namespace UserService.Core.Interfaces;

public interface IUserStatusService
{
    void EnqueueStatusUpdate(string userId, bool isActive);
}
