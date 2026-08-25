using Microsoft.AspNetCore.Mvc;
using UserService.Core.Entities;
using UserService.Core.Services;
using UserService.Core.DTOs.PasswordPolicy;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using UserService.Core.DTOs.Auth;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class PasswordPolicyController : ControllerBase
    {
        private readonly PasswordPolicyService _passwordPolicyService;
        private readonly EmailSettingsService _emailSettingsService;
        private readonly ILogger<PasswordPolicyController> _logger;

        public PasswordPolicyController(
            PasswordPolicyService passwordPolicyService,
            EmailSettingsService emailSettingsService,
            ILogger<PasswordPolicyController> logger)
        {
            _passwordPolicyService = passwordPolicyService ?? throw new ArgumentNullException(nameof(passwordPolicyService));
            _emailSettingsService = emailSettingsService ?? throw new ArgumentNullException(nameof(emailSettingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<PasswordPolicyDto>> GetCurrentPolicy()
        {
            try
            {
                var policy = await _passwordPolicyService.GetPolicyAsync();
                if (policy == null)
                {
                    return NotFound("No password policy found");
                }
                var dto = new PasswordPolicyDto
                {
                    Id = new Guid(policy.Id),
                    MinimumLength = policy.MinimumLength,
                    RequireUppercase = policy.RequireUppercase,
                    RequireLowercase = policy.RequireLowercase,
                    RequireDigit = policy.RequireDigit,
                    RequireSpecialCharacter = policy.RequireSpecialCharacter,
                    MaxAgeDays = policy.MaxAgeDays,
                    TwoFactorEnabled = policy.TwoFactorEnabled,
                    CreatedAt = policy.CreatedAt,
                    UpdatedAt = policy.UpdatedAt,
                    CreatedBy = policy.CreatedBy,
                    UpdatedBy = policy.UpdatedBy,
                    IsDeleted = policy.IsDeleted
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving password policy");
                return StatusCode(500, "An error occurred while retrieving the password policy");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePolicy(Guid id, [FromBody] PasswordPolicyUpdateDto policyDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid password policy model received");
                return BadRequest(ModelState);
            }

            try
            {
                var currentPolicy = await _passwordPolicyService.GetPolicyAsync();
                if (currentPolicy == null || new Guid(currentPolicy.Id) != id)
                {
                    return NotFound("Password policy not found");
                }

                if (policyDto.TwoFactorEnabled && !await _emailSettingsService.IsConfiguredAsync())
                {
                    return BadRequest("Configure SMTP email settings before enabling two-factor authentication — 2FA codes are delivered by email.");
                }

                currentPolicy.MinimumLength = policyDto.MinimumLength;
                currentPolicy.RequireUppercase = policyDto.RequireUppercase;
                currentPolicy.RequireLowercase = policyDto.RequireLowercase;
                currentPolicy.RequireDigit = policyDto.RequireDigit;
                currentPolicy.RequireSpecialCharacter = policyDto.RequireSpecialCharacter;
                currentPolicy.MaxAgeDays = policyDto.MaxAgeDays;
                currentPolicy.TwoFactorEnabled = policyDto.TwoFactorEnabled;

                await _passwordPolicyService.UpdatePolicyAsync(currentPolicy);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password policy");
                return StatusCode(500, "An error occurred while updating the password policy");
            }
        }
    }
}