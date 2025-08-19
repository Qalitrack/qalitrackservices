using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Services;

public interface ITwoFactorService
{
    Task<ServiceResult> GenerateAndSendCodeAsync(string userId, string email);
    Task<ServiceResult> VerifyCodeAsync(string sessionId, string code);
    Task<string> CreateTwoFactorSessionAsync(string userId);
    Task<string?> GetUserIdFromSessionAsync(string sessionId);
    Task<int> GetAttemptCountAsync(string userId);
    Task<bool> IsSessionValidAsync(string sessionId);
}