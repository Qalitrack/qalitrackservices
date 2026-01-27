using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services;

public class TwoFactorService(
    ICacheService cacheService,
    IEmailQueueService emailQueueService,
    ILogger<TwoFactorService> logger)
    : ITwoFactorService
{
    private static readonly RandomNumberGenerator SecureRandom = RandomNumberGenerator.Create();
    private const int MaxAttempts = 5;     // Maximum allowed attempts
    private const int LockoutMinutes = 10; // Lockout duration

    public async Task<ServiceResult> GenerateAndSendCodeAsync(string userId, string email)
    {
        try
        {
            // Check rate limiting - max 3 codes per 15 minutes
            var generationKey = $"2fa_generation:{userId}";
            var generationCount = await cacheService.GetAsync<int>(generationKey);

            if (generationCount >= 3)
            {
                logger.LogWarning("2FA rate limit hit for user {UserId} - {Count} codes in last 15 min", userId, generationCount);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Too many 2FA codes requested. Please wait 15 minutes before requesting another code."
                };
            }

            // Generate cryptographically secure 6-digit code
            var code = GenerateSecureCode();

            // Parallelize independent cache operations
            var codeKey = $"2fa_code:{userId}";
            var attemptKey = $"2fa_attempts:{userId}";

            logger.LogDebug("Storing 2FA code for user {UserId} → codeKey={CodeKey} (expires in 5 min)", userId, codeKey);

            var cacheOperations = new[]
            {
                cacheService.SetAsync(codeKey, code, TimeSpan.FromMinutes(5)),
                cacheService.RemoveAsync(attemptKey),
                cacheService.SetAsync(generationKey, generationCount + 1, TimeSpan.FromMinutes(15))
            };

            await Task.WhenAll(cacheOperations);

            // Queue email
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

            await emailQueueService.EnqueueEmailAsync(email, emailSubject, emailBody);

            logger.LogInformation("2FA code generated and email queued for user {UserId} - code expires in 5 min", userId);

            return new ServiceResult
            {
                Success = true,
                Message = "Verification code sent to your email address."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate/send 2FA code for user {UserId}", userId);
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
            logger.LogInformation("VERIFY-2FA ATTEMPT | sessionId = '{SessionId}' | code = '{Code}'", sessionId, code);

            // Get user ID from session
            var userId = await GetUserIdFromSessionAsync(sessionId);
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("VERIFY-2FA FAILED | Invalid or expired session | sessionId = '{SessionId}'", sessionId);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Invalid or expired session."
                };
            }

            logger.LogInformation("VERIFY-2FA | Session valid → userId = {UserId}", userId);

            // Check attempt count
            var attemptCount = await GetAttemptCountAsync(userId);
            if (attemptCount >= MaxAttempts)
            {
                logger.LogWarning("VERIFY-2FA LOCKOUT | user {UserId} - {Attempts} attempts", userId, attemptCount);
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Too many failed attempts. Please try again in {LockoutMinutes} minutes."
                };
            }

            // Get stored code
            var codeKey = $"2fa_code:{userId}";
            var storedCode = await cacheService.GetAsync<string>(codeKey);

            if (string.IsNullOrEmpty(storedCode))
            {
                logger.LogWarning("VERIFY-2FA | Code expired or missing | user {UserId} | key = {CodeKey}", userId, codeKey);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Verification code has expired. Please request a new code."
                };
            }

            logger.LogDebug("VERIFY-2FA | Stored code = '{Stored}' | Submitted = '{Submitted}'", storedCode, code);

            // Verify code
            if (storedCode != code)
            {
                await cacheService.SetAsync(
                    $"2fa_attempts:{userId}",
                    attemptCount + 1,
                    TimeSpan.FromMinutes(LockoutMinutes)
                );

                logger.LogWarning("VERIFY-2FA INVALID CODE | user {UserId} | attempts now {NewCount}/{Max}", 
                    userId, attemptCount + 1, MaxAttempts);

                return new ServiceResult
                {
                    Success = false,
                    Message = $"Invalid verification code. {MaxAttempts - attemptCount - 1} attempts remaining."
                };
            }

            // Success - clean up
            var cleanupOperations = new[]
            {
                cacheService.RemoveAsync(codeKey),
                cacheService.RemoveAsync($"2fa_attempts:{userId}"),
                cacheService.RemoveAsync($"2fa_session:{sessionId}")
            };

            await Task.WhenAll(cleanupOperations);

            logger.LogInformation("VERIFY-2FA SUCCESS | user {UserId} | session {SessionId} cleaned up", userId, sessionId);

            return new ServiceResult
            {
                Success = true,
                Message = "Verification successful."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during 2FA verification for session {SessionId}", sessionId);
            return new ServiceResult
            {
                Success = false,
                Message = "An error occurred during verification. Please try again."
            };
        }
    }

    public async Task<string> CreateTwoFactorSessionAsync(string userId)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionKey = $"2fa_session:{sessionId}";

        await cacheService.SetAsync(sessionKey, userId, TimeSpan.FromMinutes(10));

        logger.LogInformation(
            "2FA_SESSION_CREATED | key = '{SessionKey}' | userId = '{UserId}' | sessionId = '{SessionId}' | ttl = 10 min",
            sessionKey, userId, sessionId
        );

        // Optional: immediate verification read-back (remove after debugging)
        var readBack = await cacheService.GetAsync<string>(sessionKey);
        logger.LogDebug("2FA_SESSION_CREATE_READBACK | key = '{Key}' | value = '{Value}'", sessionKey, readBack ?? "NULL");

        return sessionId;
    }

    public async Task<string?> GetUserIdFromSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            logger.LogWarning("GetUserIdFromSessionAsync called with empty sessionId");
            return null;
        }

        var trimmedSessionId = sessionId.Trim();
        var sessionKey = $"2fa_session:{trimmedSessionId}";

        var userId = await cacheService.GetAsync<string>(sessionKey);

        logger.LogInformation(
            "2FA_SESSION_LOOKUP | key = '{Key}' | input = '{Input}' | trimmed = '{Trimmed}' | found userId = '{UserId}'",
            sessionKey, sessionId, trimmedSessionId, userId ?? "NULL"
        );

        return userId;
    }

    public async Task<int> GetAttemptCountAsync(string userId)
    {
        var cacheKey = $"2fa_attempts:{userId}";
        var attempts = await cacheService.GetAsync<int>(cacheKey);
        logger.LogDebug("Attempt count check for user {UserId} → {Attempts}", userId, attempts);
        return attempts;
    }

    public async Task<bool> IsSessionValidAsync(string sessionId)
    {
        var userId = await GetUserIdFromSessionAsync(sessionId);
        return !string.IsNullOrEmpty(userId);
    }

    private static string GenerateSecureCode()
    {
        var randomBytes = new byte[4];
        SecureRandom.GetBytes(randomBytes);
        var randomInt = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
        var code = (randomInt % 900000) + 100000;
        return code.ToString();
    }
}