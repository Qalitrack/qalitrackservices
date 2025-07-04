using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly IConfiguration _configuration;
    private readonly int _workFactor;
    private readonly Dictionary<string, DateTime> _passwordResetTokens;

    public PasswordService(IConfiguration configuration)
    {
        _configuration = configuration;
        _workFactor = int.Parse(_configuration["Password:WorkFactor"] ?? "12");
        _passwordResetTokens = new Dictionary<string, DateTime>();
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public bool IsPasswordValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Minimum length check
        if (password.Length < 8)
            return false;

        // Must contain at least one uppercase letter
        if (!password.Any(char.IsUpper))
            return false;

        // Must contain at least one lowercase letter
        if (!password.Any(char.IsLower))
            return false;

        // Must contain at least one digit
        if (!password.Any(char.IsDigit))
            return false;

        // Must contain at least one special character
        var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        if (!password.Any(c => specialChars.Contains(c)))
            return false;

        return true;
    }

    public string GenerateSecurePassword(int length = 12)
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        const string allChars = uppercase + lowercase + digits + specialChars;

        using var rng = RandomNumberGenerator.Create();
        var password = new StringBuilder();

        // Ensure at least one character from each category
        password.Append(GetRandomChar(rng, uppercase));
        password.Append(GetRandomChar(rng, lowercase));
        password.Append(GetRandomChar(rng, digits));
        password.Append(GetRandomChar(rng, specialChars));

        // Fill the rest with random characters from all categories
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(rng, allChars));
        }

        // Shuffle the password to avoid predictable patterns
        return ShuffleString(password.ToString(), rng);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        var token = Guid.NewGuid().ToString("N");
        var tokenKey = $"{userId}:{token}";
        
        // Token expires in 1 hour
        _passwordResetTokens[tokenKey] = DateTime.UtcNow.AddHours(1);
        
        // Clean up expired tokens
        CleanupExpiredTokens();
        
        return await Task.FromResult(token);
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token, string userId)
    {
        var tokenKey = $"{userId}:{token}";
        
        if (_passwordResetTokens.TryGetValue(tokenKey, out var expirationTime))
        {
            if (DateTime.UtcNow <= expirationTime)
            {
                return await Task.FromResult(true);
            }
            else
            {
                // Remove expired token
                _passwordResetTokens.Remove(tokenKey);
            }
        }
        
        return await Task.FromResult(false);
    }

    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        if (!IsPasswordValid(newPassword))
            return false;

        var isValidToken = await ValidatePasswordResetTokenAsync(token, userId);
        if (!isValidToken)
            return false;

        // Remove the used token
        var tokenKey = $"{userId}:{token}";
        _passwordResetTokens.Remove(tokenKey);

        return true; // In a real implementation, you would update the user's password in the database
    }

    private static char GetRandomChar(RandomNumberGenerator rng, string chars)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomIndex = BitConverter.ToUInt32(randomBytes, 0) % chars.Length;
        return chars[(int)randomIndex];
    }

    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        for (int i = array.Length - 1; i > 0; i--)
        {
            var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            var j = (int)(BitConverter.ToUInt32(randomBytes, 0) % (i + 1));
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }

    private void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = _passwordResetTokens
            .Where(kvp => kvp.Value <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var expiredToken in expiredTokens)
        {
            _passwordResetTokens.Remove(expiredToken);
        }
    }
}