namespace UserService.Core.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    bool IsPasswordValid(string password);
    string GenerateSecurePassword(int length = 12);
    Task<string> GeneratePasswordResetTokenAsync(string userId);
    Task<bool> ValidatePasswordResetTokenAsync(string token, string userId);
    Task<bool> ResetPasswordAsync(string userId, string token, string newPassword);
}