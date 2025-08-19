using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    public class PasswordPolicyService
    {
        private readonly IPasswordPolicyRepository _policyRepository;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PasswordPolicyService> _logger;
        private readonly IUserRepository _userRepository;
        private const string CacheKey = "PasswordPolicy";

        public PasswordPolicyService(
            IPasswordPolicyRepository policyRepository,
            IConfiguration configuration,
            ICacheService cacheService,
            ILogger<PasswordPolicyService> logger,
            IUserRepository userRepository)
        {
            _policyRepository = policyRepository ?? throw new ArgumentNullException(nameof(policyRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<PasswordPolicy> GetPolicyAsync()
        {
            var cachedPolicy = await _cacheService.GetAsync<PasswordPolicy>(CacheKey);
            if (cachedPolicy != null)
            {
                return cachedPolicy;
            }

            try
            {
                var dbPolicy = await _policyRepository.GetCurrentPolicyAsync();
                if (dbPolicy != null)
                {
                    await _cacheService.SetAsync(CacheKey, dbPolicy, TimeSpan.FromHours(1));
                    return dbPolicy;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve password policy from database");
            }

            // Fallback to configuration
            var fallbackPolicy = _configuration.GetSection("PasswordPolicy").Get<PasswordPolicy>();
            if (fallbackPolicy == null)
            {
                _logger.LogWarning("No password policy in configuration; using default");
                fallbackPolicy = new PasswordPolicy
                {
                    MinimumLength = 8,
                    RequireUppercase = true,
                    RequireLowercase = true,
                    RequireDigit = true,
                    RequireSpecialCharacter = true,
                    MaxAgeDays = 90
                };
            }

            await _cacheService.SetAsync(CacheKey, fallbackPolicy, TimeSpan.FromHours(1));
            return fallbackPolicy;
        }

        public async Task UpdatePolicyAsync(PasswordPolicy policy)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            try
            {
                await _policyRepository.UpdatePolicyAsync(policy);
                await _cacheService.SetAsync(CacheKey, policy, TimeSpan.FromHours(1));
                _logger.LogInformation("Password policy updated and cached");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update password policy");
                throw;
            }
        }

        public async Task<ValidationResult> ValidatePasswordAsync(string password, string userId = null)
        {
            var policy = await GetPolicyAsync();
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password))
                errors.Add("Password is required.");
            if (policy!.MinimumLength > 0 && password.Length < policy.MinimumLength)
                errors.Add($"Password must be at least {policy.MinimumLength} characters long.");
            if (policy.RequireUppercase && !password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter.");
            if (policy.RequireLowercase && !password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter.");
            if (policy.RequireDigit && !password.Any(char.IsDigit))
                errors.Add("Password must contain at least one digit.");
            if (policy.RequireSpecialCharacter && !password.Any(c => "!@#$%^&*()".Contains(c)))
                errors.Add("Password must contain at least one special character.");

            // Optional: Check password history or reuse
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userRepository.GetByIdAsync(userId, true);
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                    errors.Add("New password cannot be the same as the old password.");
            }

            return errors.Any() ? new ValidationResult { IsValid = false, Errors = errors } : new ValidationResult { IsValid = true };
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}