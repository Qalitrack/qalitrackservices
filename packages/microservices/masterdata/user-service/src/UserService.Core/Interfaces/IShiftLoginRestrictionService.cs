namespace UserService.Core.Interfaces;

public interface IShiftLoginRestrictionService
{
    Task<(bool IsAllowed, string Reason)> CanUserLoginAsync(string userId);
}