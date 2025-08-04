using Microsoft.AspNetCore.Mvc;
using UserService.Core.Entities;
using UserService.Core.Services;
using UserService.Core.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordPolicyController : ControllerBase
    {
        private readonly PasswordPolicyService _passwordPolicyService;
        private readonly ILogger<PasswordPolicyController> _logger;

        public PasswordPolicyController(
            PasswordPolicyService passwordPolicyService,
            ILogger<PasswordPolicyController> logger)
        {
            _passwordPolicyService = passwordPolicyService ?? throw new ArgumentNullException(nameof(passwordPolicyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<PasswordPolicy>> GetCurrentPolicy()
        {
            try
            {
                var policy = await _passwordPolicyService.GetPolicyAsync();
                if (policy == null)
                {
                    _logger.LogInformation("No password policy found");
                    return NotFound("No password policy found");
                }
                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving password policy");
                return StatusCode(500, "An error occurred while retrieving the password policy");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdatePolicy([FromBody] PasswordPolicy policy)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid password policy model received");
                return BadRequest(ModelState);
            }

            try
            {
                // Since only one policy is allowed, we'll update the existing one or create a new one
                var currentPolicy = await _passwordPolicyService.GetPolicyAsync();
                if (currentPolicy != null)
                {
                    policy.Id = currentPolicy.Id; // Ensure we update the existing policy
                }

                await _passwordPolicyService.UpdatePolicyAsync(policy);
                _logger.LogInformation("Password policy updated successfully");
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