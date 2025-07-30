using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Interfaces;

namespace UserService.Core.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly ICacheService _cacheService;
    private readonly IEmailQueueService _emailQueueService;
    private readonly ILogger<TwoFactorService> _logger;
    private static readonly RandomNumberGenerator _secureRandom = RandomNumberGenerator.Create();

    public TwoFactorService(
        ICacheService cacheService,
        IEmailQueueService emailQueueService,
        ILogger<TwoFactorService> logger)
    {
        _cacheService = cacheService;
        _emailQueueService = emailQueueService;
        _logger = logger;
    }

    public async Task<ServiceResult> GenerateAndSendCodeAsync(string userId, string email)
    {
        try
        {
            // Check rate limiting - max 3 codes per 15 minutes
            var generationKey = $"2fa_generation:{userId}";
            var generationCount = await _cacheService.GetAsync<int>(generationKey);
            
            if (generationCount >= 3)
            {
                _logger.LogWarning("Rate limit exceeded for 2FA code generation for user {UserId}", userId);
                return new ServiceResult 
                { 
                    Success = false, 
                    Message = "Too many 2FA codes requested. Please wait 15 minutes before requesting another code." 
                };
            }

            // Generate cryptographically secure 6-digit code
            var code = GenerateSecureCode();
            
            // Parallelize independent cache operations for better performance
            var codeKey = $"2fa_code:{userId}";
            var attemptKey = $"2fa_attempts:{userId}";
            
            var cacheOperations = new[]
            {
                _cacheService.SetAsync(codeKey, code, TimeSpan.FromMinutes(5)),
                _cacheService.RemoveAsync(attemptKey),
                _cacheService.SetAsync(generationKey, generationCount + 1, TimeSpan.FromMinutes(15))
            };
            
            await Task.WhenAll(cacheOperations);

            // Queue email for background processing (non-blocking)
            var emailSubject = "Your QaliTrack Verification Code";
            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>QaliTrack Security Verification</h2>
                    <p>Your verification code is:</p>
                    <h1 style='color: #2196F3; font-size: 32px; text-align: center; padding: 20px; background-color: #f5f5f5; border-radius: 8px;'>{code}</h1>
                    <p>This code will expire in <strong>5 minutes</strong>.</p>
                    <p>If you didn't request this code, please contact your system administrator immediately.</p>
                    <br>
                    <p>Best regards,<br>QaliTrack Security Team</p>
                </body>
                </html>";

            await _emailQueueService.EnqueueEmailAsync(email, emailSubject, emailBody);

            _logger.LogInformation("2FA code generated and email queued for user {UserId}", userId);
            
            return new ServiceResult 
            { 
                Success = true, 
                Message = "Verification code sent to your email address." 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating 2FA code for user {UserId}", userId);
            return new ServiceResult 
            { 
                Success = false, 
                Message = "Failed to generate verification code. Please try again." 
            };
        }
    }

    public async Task<ServiceResult> VerifyCodeAsync(string sessionId, string code)
    {
        try
        {
            // Get user ID from session
            var userId = await GetUserIdFromSessionAsync(sessionId);
            if (string.IsNullOrEmpty(userId))
            {
                return new ServiceResult 
                { 
                    Success = false, 
                    Message = "Invalid or expired session." 
                };
            }

            // Check attempt count - max 5 attempts per code
            var attemptKey = $"2fa_attempts:{userId}";
            var attemptCount = await _cacheService.GetAsync<int>(attemptKey);
            
            if (attemptCount >= 5)
            {
                _logger.LogWarning("Too many 2FA verification attempts for user {UserId}", userId);
                return new ServiceResult 
                { 
                    Success = false, 
                    Message = "Too many failed attempts. Please request a new verification code." 
                };
            }

            // Get stored code
            var codeKey = $"2fa_code:{userId}";
            var storedCode = await _cacheService.GetAsync<string>(codeKey);
            
            if (string.IsNullOrEmpty(storedCode))
            {
                return new ServiceResult 
                { 
                    Success = false, 
                    Message = "Verification code has expired. Please request a new code." 
                };
            }

            // Increment attempt count
            await _cacheService.SetAsync(attemptKey, attemptCount + 1, TimeSpan.FromMinutes(5));

            // Verify code
            if (storedCode != code)
            {
                _logger.LogWarning("Invalid 2FA code provided for user {UserId}. Attempt {AttemptCount}/5", userId, attemptCount + 1);
                return new ServiceResult 
                { 
                    Success = false, 
                    Message = $"Invalid verification code. {4 - attemptCount} attempts remaining." 
                };
            }

            // Success - clean up in parallel
            var cleanupOperations = new[]
            {
                _cacheService.RemoveAsync(codeKey),
                _cacheService.RemoveAsync(attemptKey),
                _cacheService.RemoveAsync(sessionId)
            };
            
            await Task.WhenAll(cleanupOperations);

            _logger.LogInformation("2FA verification successful for user {UserId}", userId);
            
            return new ServiceResult 
            { 
                Success = true, 
                Message = "Verification successful." 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code for session {SessionId}", sessionId);
            return new ServiceResult 
            { 
                Success = false, 
                Message = "Verification failed. Please try again." 
            };
        }
    }

    public async Task<string> CreateTwoFactorSessionAsync(string userId)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionKey = $"2fa_session:{sessionId}";
        
        // Store session with 10-minute expiration
        await _cacheService.SetAsync(sessionKey, userId, TimeSpan.FromMinutes(10));
        
        _logger.LogInformation("2FA session created for user {UserId} with session {SessionId}", userId, sessionId);
        
        return sessionId;
    }

    public async Task<string?> GetUserIdFromSessionAsync(string sessionId)
    {
        var sessionKey = $"2fa_session:{sessionId}";
        return await _cacheService.GetAsync<string>(sessionKey);
    }

    public async Task<int> GetAttemptCountAsync(string userId)
    {
        var attemptKey = $"2fa_attempts:{userId}";
        return await _cacheService.GetAsync<int>(attemptKey);
    }

    public async Task<bool> IsSessionValidAsync(string sessionId)
    {
        var userId = await GetUserIdFromSessionAsync(sessionId);
        return !string.IsNullOrEmpty(userId);
    }

    /// <summary>
    /// Generates a cryptographically secure 6-digit code for 2FA
    /// </summary>
    /// <returns>6-digit numeric code as string</returns>
    private static string GenerateSecureCode()
    {
        // Generate 4 random bytes (32 bits)
        var randomBytes = new byte[4];
        _secureRandom.GetBytes(randomBytes);
        
        // Convert to unsigned integer and ensure positive value
        var randomInt = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
        
        // Generate 6-digit code (100000-999999)
        var code = (randomInt % 900000) + 100000;
        
        return code.ToString();
    }
}