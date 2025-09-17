namespace UserService.Core.Interfaces.Services;

public interface IShiftLoginRestrictionService
{
    Task<(bool IsAllowed, string Reason)> CanUserLoginAsync(string userId);
    Task HandleUserLogoutAsync(string userId);
}